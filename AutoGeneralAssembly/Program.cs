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
                // generate flip mates array
                // assumes first line is always a path
                var dimensions = new string[appDataLines.Length - 1];
                index = 0;
                foreach (string line in appDataLines)
                {
                    if (line.Contains("Distance"))
                    {
                        dimensions[index++] = line.Trim();
                    }
                }

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
                    } else
                    {
                        features = (Feature)features.GetNextFeature();
                    }
                }

                // iterate through mate list and flip those that correspond with the
                // distance list item
                var dimensionsIndex = dimensions.Length;
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

                // write to assembly config the opposite negation state for those distances
                // generate line number - negation flip dictionary
                // compare distance list to the first instance of an identifying line
                // then identify that dimension's negation state line
                
            }

            // rebuild
            //model.ForceRebuild3(true);
            
            // if rebuild app data contains a dimension list - creates a new array for the mates that need to be flipped
            /*if (rebuildAppDataLines.Length >= 2)
            {
                if (rebuildAppDataLines[1].Contains("Distance"))
                {
                    matesToFlip = new string[rebuildAppDataLines.Length - 1];
                    for (var i = 1; i < rebuildAppDataLines.Length; ++i)
                    {
                        matesToFlip[i - 1] = rebuildAppDataLines[i];
                    }
                    // flips the mate if the X/Z offset is negative relative to current position
                    var cutOff = 5_000;
                    var firstFeature = (Feature)model.FirstFeature();
                    while (firstFeature != null && cutOff-- > 0)
                    {
                        if ("MateGroup" == firstFeature.GetTypeName())
                        {
                            var mateGroup = (Feature)firstFeature.GetFirstSubFeature();
                            var index = 0;
                            while (mateGroup != null)
                            {
                                var mate = (Mate2)mateGroup.GetSpecificFeature2();
                                var mateName = mateGroup.Name;
                                foreach (string dimension in matesToFlip)
                                {
                                    if (dimension == mateName)
                                    {
                                        mate.Flipped = !mate.Flipped;
                                    }
                                }

                                mateGroup = (Feature)mateGroup.GetNextSubFeature();
                                ++index;
                            }
                        }
                        firstFeature = (Feature)firstFeature.GetNextFeature();
                    }

                    // remove the listed mates so it doesn't flip them again
                    System.IO.File.WriteAllText(rebuildAppDataPath, assemblyConfigPath);
                }*/
        }
    }
}
