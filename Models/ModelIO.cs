using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlaylistEditor.Models
{
    public class ModelIO {
        public static void SaveProject(Project project)
        {
            var options = new JsonSerializerOptions {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            };
            string jsonString = JsonSerializer.Serialize(project, options);
            File.WriteAllText(project.contentsFile, jsonString);
        }

        public static Project LoadProject(string projectDirectory)
        {
            string filename = Path.Join(projectDirectory, Project.ProjectContentsFileLeaf);
            string jsonString = File.ReadAllText(filename);
            var options = new JsonSerializerOptions {
                IncludeFields = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            };
            Project project = JsonSerializer.Deserialize<Project>(jsonString, options);
            return project;
        }
    }
}
