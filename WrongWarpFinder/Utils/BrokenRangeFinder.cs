using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using WrongWarpFinder.Shapes;

namespace WrongWarpFinder.Utils;

public static class BrokenRangeFinder
{
    public static void ScanAll(string fileName = "planmap")
    {
        var sheet = Plugin.DataManager.GetExcelSheet<TerritoryType>();
        foreach (var row in sheet)
        {
            var broken = ScanTerritory(row.RowId, fileName);
            if (broken.Count == 0) continue;
            
            foreach (var obj in broken)
            {
                Plugin.ChatGui.Print($"{obj.Transform.Translation.X},{obj.Transform.Translation.Y},{obj.Transform.Translation.Z}");
            }
        }
    }

    public static void LogTerritoryInfo(uint id)
    {
        var row = Plugin.DataManager.GetExcelSheet<TerritoryType>().FirstOrNull(x => x.RowId == id);
        if (row == null) return;
        
        Plugin.ChatGui.Print(row.Value.Name.ToString() + " " + row.Value.PlaceName.Value.Name.ToString());
    }

    public static void ScanForBrokenReturnIds()
    {
        var sheet = Plugin.DataManager.GetExcelSheet<TerritoryType>();

        // returnInstanceId, destInstanceId, territory
        List<(uint, uint, string)> ids = [];
        
        foreach (var row in sheet)
        {
            var ranges = GetExitRanges(row.RowId);
            foreach (var range in ranges)
            {
                ids.Add((range.ReturnInstanceId, range.DestInstanceId, row.Name.ExtractText() + " " + row.PlaceName.Value.Name.ToString()));
            }
        }
        
        // Check if there are any mismatches
        foreach (var exit in ids)
        {
            bool hasReturnId = ids.Any(x => x.Item1 == exit.Item2);
            if (!hasReturnId)
            {
                Plugin.ChatGui.Print($"Return ID does not exist! {exit.Item3}");
            }

            if (exit.Item1 == exit.Item2)
            {
                Plugin.ChatGui.Print($"Destination and Return ID's match! {exit.Item3}");
            }
        }
    }

    public static string[] FileNames = [
        "planmap",
        "planevent",
        "planlive",
        "planner"
    ];

    public static List<LayerCommon.ExitRangeInstanceObject> GetExitRanges(uint id)
    {
        var row = Plugin.DataManager.GetExcelSheet<TerritoryType>().FirstOrNull(x => x.RowId == id);
        if (row == null) return [];
        
        var bgPath = row.Value.Bg.ToString();
        var lastSeparatorPos = bgPath.LastIndexOf('/');
        
        if (lastSeparatorPos == -1) return [];

        List<LayerCommon.ExitRangeInstanceObject> result = [];
        
        var filePath = $"bg/{bgPath[..lastSeparatorPos]}/planmap.lgb";
        var lgb = Plugin.DataManager.GameData.GetFile<LgbFile>(filePath);

        if (lgb is null) return [];
        
        Plugin.Log.Verbose($"Reading {id} {row.Value.PlaceName.Value.Name.ToString()} planmap.lgb file");
        
        foreach (var layer in lgb.Layers)
        {
            Plugin.Log.Verbose($"Layer: {layer.Name}");
            var objects = layer.InstanceObjects;
            
            foreach (var obj in objects)
            {
                Plugin.Log.Verbose($"Object: {obj.AssetType}");
                if (obj.AssetType != LayerEntryType.ExitRange) continue;
                result.Add((LayerCommon.ExitRangeInstanceObject)obj.Object);
            }
        }
        
        return result;
    }
    
    public static List<LayerCommon.InstanceObject> ScanTerritory(uint id, string fileName = "planmap", bool showAll = false)
    {
        var row = Plugin.DataManager.GetExcelSheet<TerritoryType>().FirstOrNull(x => x.RowId == id);
        if (row == null) return [];
        
        var bgPath = row.Value.Bg.ToString();
        var lastSeparatorPos = bgPath.LastIndexOf('/');
        
        if (lastSeparatorPos == -1) return [];

        List<LayerCommon.InstanceObject> result = [];
        
        var filePath = $"bg/{bgPath[..lastSeparatorPos]}/{fileName}.lgb";
        var lgb = Plugin.DataManager.GameData.GetFile<LgbFile>(filePath);

        if (lgb is null) return [];
        
        Plugin.Log.Verbose($"Reading {id} {row.Value.PlaceName.Value.Name.ToString()} {fileName}.lgb file");
        
        foreach (var layer in lgb.Layers)
        {
            Plugin.Log.Verbose($"Layer: {layer.Name}");
            var objects = layer.InstanceObjects;
            
            foreach (var obj in objects)
            {
                Plugin.Log.Verbose($"Object: {obj.AssetType}");
                if (obj.AssetType != LayerEntryType.ExitRange) continue;
                
                var exit = (LayerCommon.ExitRangeInstanceObject)obj.Object;
                
                if (exit.DestInstanceId == 0 || showAll)
                {
                    result.Add(obj);
                }
            }
        }
        
        return result;
    }
}
