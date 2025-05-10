using System.Globalization;
using System.Text.RegularExpressions;

namespace Triangle3DAnimation.ObjLoader
{
    public class ObjLoader
    {
        public static ObjAnimation ParseObjAnimation(string workingDirectory, string fileName)
        {
            ObjAnimation objAnimation = new();
            string[] files = Directory.GetFiles(workingDirectory, fileName + "*", SearchOption.TopDirectoryOnly);

            if (files.Length <= 0)
            {
                throw new FileNotFoundException();
            }

            Regex pattern = new(Regex.Escape(workingDirectory) + "\\\\" + Regex.Escape(fileName) + "(?<frameNumber>\\d+)\\.obj");
            foreach (string filePath in files)
            {
                Match match = pattern.Match(filePath);
                if (match.Success)
                {
                    int frameNumber = int.Parse(match.Groups["frameNumber"].Value);
                    ObjModel objModelForThisFrame = ParseObj(workingDirectory, fileName + frameNumber);
                    objAnimation.Frames[frameNumber] = objModelForThisFrame;
                }
            }

            return objAnimation;
        }

        public static ObjModel ParseObj(string workingDirectory, string fileName)
        {
            ObjModelBuilder objModelBuilder = new();
            var objPath = Path.Combine(workingDirectory, fileName + ".obj");
            using (var reader = new StreamReader(objPath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    ParseLine(line, objModelBuilder, workingDirectory);
                }
            }
            return objModelBuilder.Build();
        }

        private static void ParseLine(string line, ObjModelBuilder objModelBuilder, string workingDirectory)
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
                case "v":
                    ParseVertex(tokens, objModelBuilder);
                    break;
                case "f":
                    ParseFace(tokens, objModelBuilder);
                    break;
                case "usemtl":
                    if (tokens.Length < 2)
                    {
                        throw new ArgumentException("error: usemtl must define the material name to be used");
                    }
                    objModelBuilder.setCurrentMaterial(tokens[1]);
                    break;
                case "mtllib":
                    foreach (string materialFileName in tokens.Skip(1).Take(tokens.Length - 1))
                    {
                        ParseMtl(Path.Combine(workingDirectory, materialFileName), objModelBuilder);
                    }
                    break;
                default:
                    // other types of data are not supported for 3D Triangles in Trackmania
                    break;
            }
        }

        private static void ParseVertex(string[] tokens, ObjModelBuilder objModelBuilder)
        {
            if (tokens.Length < 4)
            {
                throw new ArgumentException("error : vertex must have x, y and z value");
            }

            float x = float.Parse(tokens[1], CultureInfo.InvariantCulture);
            float y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
            float z = float.Parse(tokens[3], CultureInfo.InvariantCulture);
            // w is ignored

            objModelBuilder.AddVertex(x, y, z);
        }

        private static void ParseFace(string[] tokens, ObjModelBuilder objModelBuilder)
        {
            if (tokens.Length < 4)
            {
                throw new ArgumentException("error : face must have at least 3 vertices");
            }

            List<ObjFaceVertex> facesVertices = [];
            foreach (string faceVertexDefinition in tokens.Skip(1).Take(tokens.Length - 1))
            {
                facesVertices.Add(ParseFaceVertex(faceVertexDefinition, objModelBuilder));
            }
            objModelBuilder.AddFace(facesVertices);
        }

        private static ObjFaceVertex ParseFaceVertex(string faceVertexDefinition, ObjModelBuilder objModelBuilder)
        {
            // Split on '/' or '//' using Regex for consistent behavior
            var tokens = Regex.Split(faceVertexDefinition, @"//|/").Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();

            if (tokens.Length >= 1)
            {
                if (!int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int vertexIndex))
                    throw new ArgumentException($"Invalid vertex index: '{tokens[0]}'");

                return new ObjFaceVertex(objModelBuilder.GetVertex(vertexIndex));
            }
            else
            {
                throw new ArgumentException("Error: a face vertex must reference at least one vertex.");
            }
        }

        private static void ParseMtl(string filePath, ObjModelBuilder objModelBuilder)
        {
            foreach (ObjMaterial objMaterial in MtlLoader.MtlLoader.ParseMtl(filePath))
            {
                objModelBuilder.Materials.Add(objMaterial);
            }
        }
    }
}
