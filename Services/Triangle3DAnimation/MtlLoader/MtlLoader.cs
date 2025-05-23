﻿using System.Globalization;
using Triangle3DAnimation.ObjLoader;

namespace Triangle3DAnimation.MtlLoader
{
    public static class MtlLoader
    {

        public static List<ObjMaterial> ParseMtl(string filePath)
        {
            List<ObjMaterial> materials = [];
            ObjMaterial? currentMaterial = null;
            if (!File.Exists(filePath))
            {
                return materials;
            }

			string[] fileLines = File.ReadAllLines(filePath);
            foreach (string line in fileLines)
            {
                ParseLine(line, filePath, materials, ref currentMaterial);
            }
            
            if (currentMaterial != null)
            {
                materials.Add(currentMaterial);
            }
            return materials;
        }

        private static void ParseLine(string line, string filePath, List<ObjMaterial> materials, ref ObjMaterial? currentMaterial)
        {
			string[] separators = [" ", "\t"];
			string[] tokens = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
            {
                return; // empty line
            }

            // check keyword
            switch (tokens[0])
            {
                case "newmtl":
                    if (tokens.Length < 2)
                    {
                        throw new ArgumentException("error: newmtl must define the material name");
                    }

                    if (currentMaterial != null)
                    {
                        materials.Add(currentMaterial);
                    }   
                    currentMaterial = new ObjMaterial(tokens[1]);
                    break;
                case "Kd":
                    if (currentMaterial == null)
                    {
                        throw new ArgumentException("error: Kd statement must be used after a newmtl was define");
                    }
                    if (tokens.Length == 2)
                    {
                        currentMaterial.DiffuseR = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                        currentMaterial.DiffuseG = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                        currentMaterial.DiffuseB = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                    } else if (tokens.Length == 4) 
                    {
                        currentMaterial.DiffuseR = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                        currentMaterial.DiffuseG = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                        currentMaterial.DiffuseB = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                    } else
                    {
                        throw new ArgumentException("error: Kd statment must define 1 value (RGB all at once) or 3 values (RGB)");
                    }
                    break;
                default:
                    // other types of data are not supported for 3D Triangles in Trackmania
                    break;
            }
        }
    }
}
