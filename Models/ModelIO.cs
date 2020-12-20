using System.IO;
using System.Text.Json;
using PlaylistEditor.Models;

namespace PlaylistEditor.Models
{
    public class ModelIO {
        public static void SaveProject(Project project)
        {
            var options = new JsonSerializerOptions {
                WriteIndented = true,
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
            };
            Project project = JsonSerializer.Deserialize<Project>(jsonString, options);
            return project;
        }
    }
}
