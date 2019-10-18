using SldWorks;
using System;
using SwConst;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGeneralAssembly
{
    class Program
    {
        static void Main(string[] args)
        {
            var noFlipDebug = true;
            var noRebuildDebug = true;
            var swInstance = new SldWorks.SldWorks();
            var model = (ModelDoc2)swInstance.ActiveDoc;

            var appDataPath = @"C:\Users\bolinger\Documents\SolidWorks Projects\Prefab Blob - Cover Blob\app data\rebuild.txt";
            var appDataLines = System.IO.File.ReadAllLines(appDataPath);

            // determine if mate flip is requested
            var hasMateFlips = false;
            var index = 0;
            foreach (string line in appDataLines)
            {
                if (index > 0 && !hasMateFlips && line.Contains("Distance"))
                {
                    hasMateFlips = true;
                }
                ++index;
            }
            
            // if flip mates
            // flip the mates in the list
            // -> remove mate flip list from app data
            // -> write the oppisite negation state values to the assembly config file
            if (hasMateFlips)
            {
                var dimensions = new string[appDataLines.Length - 1];
                var dimensionsIndex = dimensions.Length;
                
                // generate flip mates array
                // assumes first line is always a path
                index = 0;
                foreach (string line in appDataLines)
                    {
                        if (line.Contains("Distance"))
                        {
                            dimensions[index++] = line.Trim();
                        }
                    }

                if (!noFlipDebug)
                {
                    // get mate list reference from the list of top-level features
                    var features = (Feature)model.FirstFeature();
                    Feature mates = default(Feature);
                    var stop = true;
                    while (features != null && stop)
                    {
                        if ("MateGroup" == features.GetTypeName())
                        {
                            mates = (Feature)features.GetFirstSubFeature();
                            stop = false;
                        }
                        else
                        {
                            features = (Feature)features.GetNextFeature();
                        }
                    }

                    // iterate through mate list and flip those that correspond with the
                    // distance list item
                    while (mates != null && dimensionsIndex > 0)
                    {
                        var mate = (Mate2)mates.GetSpecificFeature2();
                        var mateName = mates.Name.Trim();
                        foreach (string dimension in dimensions)
                        {
                            if (mateName.Contains(dimension))
                            {
                                mate.Flipped = !mate.Flipped;
                                --dimensionsIndex;
                            }
                        }

                        mates = (Feature)mates.GetNextSubFeature();
                    }
                }

                // write to assembly config the opposite negation state for those distances
                // generate line number - negation flip dictionary
                // compare distance list to the first instance of an identifying line
                // then identify that dimension's negation state line
                var assemblyConfigPath = appDataLines[0];
                var assemblyConfigLines = System.IO.File.ReadAllLines(assemblyConfigPath);
                
                // populate assembly feature list
                dimensionsIndex = dimensions.Length;
                index = 0;
                var featureList = new string[dimensions.Length];
                var featureListIndex = 0;
                foreach (string line in assemblyConfigLines)
                {
                    foreach (string dimension in dimensions)
                    {
                        if (line.Contains(dimension))
                        {
                            featureList[featureListIndex++] = line.Split('=')[1]
                                .Replace("Offset", "")
                                .Replace("\"", "").Trim();
                            if (--dimensionsIndex == 0)
                            {
                                break;
                            }
                        }
                            ++index;
                    }
                }
                
                // generate new assembly configfiles
                for (var i = 0; i < assemblyConfigLines.Length; ++i)
                {

                }
            }

            // rebuild
            if (!noRebuildDebug)
            {
                model.ForceRebuild3(true);
            }
        }
    }
}
