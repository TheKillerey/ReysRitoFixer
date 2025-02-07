using System.Diagnostics;
using System.Text.RegularExpressions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\n--- Welcome to PatchFixer by ProjectRey ---");
            Console.WriteLine("1. Run PatchFixer 25.S1.3");
            Console.WriteLine("2. Run PatchFixer 25.S1.4 (PBE)");
            Console.WriteLine("3. Exit");

            Console.Write("\nChoose an option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await RunPatchFixer25S13();
                    break;
                case "2":
                    await RunPatchFixer25S14PBE();
                    break;
                case "3":
                    Console.WriteLine("Exiting the program. Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please select '1', '2', or '3'.");
                    break;
            }
        }
    }
    
    private static void ValidateFilePermissions(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            if (!HasFileAccess(file))
            {
                Console.WriteLine($"WARNING: Insufficient permissions to access or modify file: {file}");
            }
        }
    }

    private static bool HasFileAccess(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Version 25.S1.3: Current logic with one replacement
    private static async Task RunPatchFixer25S13()
    {
        Console.WriteLine("\nStarting PatchFixer 25.S1.3...");
        await RunPatchFixer(("textureName: string =", "texturePath: string ="),
            ("TextureName: string =", "texturePath: string ="), ("texturename: string =", "texturePath: string ="));
    }

    // Version 25.S1.4 (PBE): Includes additional replacement logic
    private static async Task RunPatchFixer25S14PBE()
    {
        Console.WriteLine("\nStarting PatchFixer 25.S1.4 (PBE)...");
        await RunPatchFixer(("textureName: string =", "texturePath: string ="),
            ("TextureName: string =", "texturePath: string ="), ("texturename: string =", "texturePath: string ="),
            ("samplerName: string =", "textureName: string ="), ("SamplerName: string =", "textureName: string ="),
            ("samplername: string =", "textureName: string ="));
    }

    // Generic PatchFixer logic with flexible replacement patterns
    private static async Task RunPatchFixer(params (string find, string replace)[] replacements)
    {
        // Step 1: Ensure CSLoL Manager
        var cslolPath = GetCslolManagerPath();
        if (cslolPath == null)
        {
            Console.WriteLine("CSLoL Manager is not running. Please start it and try again.");
            return;
        }

        var managerPath = Path.GetDirectoryName(cslolPath) ?? "";
        var modsDirectory = Path.Combine(managerPath, "installed");
        var hashesDirectory = Path.Combine("OtherTools", "hashes");

        // Step 2: Ensure hash files exist
        await EnsureHashFiles(hashesDirectory);

        Console.WriteLine("\nBacking up modified files...");
        var backupPath = Path.Combine(managerPath, "installed_backup");
        BackupInstalledFolder(modsDirectory, backupPath);

        // Step 3: Extract WAD files
        Console.WriteLine("\nExtracting WAD files...");
        var wadExtractPath = Path.Combine(hashesDirectory, "wad-extract.exe");
        var wadFiles = FindWadFiles(modsDirectory);
        await ExportWadFilesParallel(wadExtractPath, wadFiles);

        // Step 4: Modify extracted data
        Console.WriteLine("\nModifying extracted data...");
        var ritobinPath = Path.Combine("OtherTools", "ritobin_cli.exe");
        var binFiles = FindBinFiles(modsDirectory);
        await ConvertBinToPyParallel(ritobinPath, binFiles);

        var pyFiles = Directory.GetFiles(modsDirectory, "*.py", SearchOption.AllDirectories);

        // Check for file permissions
        ValidateFilePermissions(pyFiles);

        // Apply replacements
        ReplaceInPyFiles(pyFiles, replacements);

        await ConvertPyToBinParallel(ritobinPath, pyFiles);

        // Step 5: Repack into WAD files
        Console.WriteLine("\nRepacking WAD files...");
        var wadMakePath = Path.Combine(hashesDirectory, "wad-make.exe");
        await RepackWadClientFiles(wadMakePath, wadFiles);

        Console.WriteLine("\nPatchFixer completed successfully!");
    }

    // Updated ReplaceInPyFiles method to display progress
    private static void ReplaceInPyFiles(IEnumerable<string> pyFiles, (string find, string replace)[] replacements)
    {
        var pyFileList = pyFiles.ToList();
        var totalFiles = pyFileList.Count;
        var processedFiles = 0;

        Parallel.ForEach(pyFileList, pyFile =>
        {
            try
            {
                if (!HasFileAccess(pyFile))
                {
                    Console.WriteLine($"Permission denied for file: {pyFile}");
                    return;
                }

                var content = File.ReadAllText(pyFile);
                foreach (var (find, replace) in replacements)
                {
                    // Use case-insensitive Regex.Replace
                    content = Regex.Replace(content, find, replace, RegexOptions.IgnoreCase);
                }

                File.WriteAllText(pyFile, content);
                Console.WriteLine($"Updated {pyFile}");
                Interlocked.Increment(ref processedFiles);

                // Calculate and display progress
                var progress = processedFiles * 100 / totalFiles;
                Console.WriteLine($"Progress: {progress}%");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Permission Error: Cannot update file {pyFile}. Details: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File Error: Cannot access file {pyFile}. Details: {ex.Message}");
            }
        });
    }
    

    // Ensure hash files exist (download missing ones)
    private static async Task EnsureHashFiles(string hashesDirectory)
    {
        string[] hashFiles = 
        {
            "hashes.game.txt",
            "hashes.lcu.txt",
            "hashes.binfields.txt",
            "hashes.bintypes.txt",
            "hashes.binhashes.txt",
            "hashes.binentries.txt"
        };

        var allHashesExist = hashFiles.All(hash => File.Exists(Path.Combine(hashesDirectory, hash)));
        if (!allHashesExist)
        {
            Console.WriteLine("\nMissing hash files detected. Downloading...");
            await DownloadHashFilesInParallel(
                hashFiles,
                "https://raw.communitydragon.org/data/hashes/lol/",
                hashesDirectory);
        }
        else
        {
            Console.WriteLine("All required hash files are present.");
        }
    }

    // Find .wad.client files
    private static List<string> FindWadFiles(string directory)
    {
        return Directory.GetFiles(directory, "*.wad.client", SearchOption.AllDirectories).ToList();
    }

    // Find .bin files
    private static List<string> FindBinFiles(string directory)
    {
        return Directory.GetFiles(directory, "*.bin", SearchOption.AllDirectories).ToList();
    }

    // Extract WAD files
    private static async Task ExportWadFilesParallel(string wadExtractPath, List<string> wadFiles)
    {
        var totalFiles = wadFiles.Count;
        var processedFiles = 0;

        var tasks = wadFiles.Select(wadFile =>
        {
            return Task.Run(() =>
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(wadFile) ?? "",
                    Path.GetFileNameWithoutExtension(wadFile));
                Process.Start(new ProcessStartInfo
                {
                    FileName = wadExtractPath,
                    Arguments = $"\"{wadFile}\" \"{outputFolder}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
                Interlocked.Increment(ref processedFiles);

                // Calculate and display progress
                var progress = processedFiles * 100 / totalFiles;
                Console.WriteLine($"Extracting WAD Files Progress: {progress}%");
            });
        });
        await Task.WhenAll(tasks);
    }

// Updated ConvertBinToPyParallel method
    private static async Task ConvertBinToPyParallel(string ritobinPath, List<string> binFiles)
    {
        var totalFiles = binFiles.Count;
        var processedFiles = 0;

        var tasks = binFiles.Select(binFile =>
        {
            return Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ritobinPath,
                    Arguments = $"\"{binFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
                Interlocked.Increment(ref processedFiles);

                // Calculate and display progress
                var progress = processedFiles * 100 / totalFiles;
                Console.WriteLine($"Converting BIN to PY Progress: {progress}%");
            });
        });
        await Task.WhenAll(tasks);
    }

    // Convert PY to BIN
    private static async Task ConvertPyToBinParallel(string ritobinPath, IEnumerable<string> pyFiles)
    {
        var tasks = pyFiles.Select(pyFile =>
        {
            return Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ritobinPath,
                    Arguments = $"\"{pyFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
                File.Delete(pyFile);
            });
        });
        await Task.WhenAll(tasks);
    }

    // Repack WAD files
    private static async Task RepackWadClientFiles(string wadMakePath, List<string> wadFiles)
    {
        var tasks = wadFiles.Select(wadFile =>
        {
            return Task.Run(() =>
            {
                var extractedFolder = Path.Combine(Path.GetDirectoryName(wadFile) ?? "",
                    Path.GetFileNameWithoutExtension(wadFile));
                Process.Start(new ProcessStartInfo
                {
                    FileName = wadMakePath,
                    Arguments = $"\"{extractedFolder}\" \"{wadFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
                Directory.Delete(extractedFolder, true);
            });
        });
        await Task.WhenAll(tasks);
    }

    // Backup folder
    private static void BackupInstalledFolder(string source, string backup)
    {
        if (Directory.Exists(backup))
            Directory.Delete(backup, true);
        DirectoryCopy(source, backup);
    }

    private static void DirectoryCopy(string source, string dest)
    {
        foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(source, dest));
        foreach (var file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(source, dest), true);
    }

    // Get CSLoL Manager path
    private static string? GetCslolManagerPath()
    {
        return Process.GetProcessesByName("cslol-manager").FirstOrDefault()?.MainModule?.FileName;
    }

    // Download missing files
    private static async Task DownloadHashFilesInParallel(string[] filenames, string baseUrl, string directory)
    {
        using var httpClient = new HttpClient();
        Directory.CreateDirectory(directory);
        var tasks = filenames.Select(async file =>
        {
            var url = $"{baseUrl}/{file}";
            var dest = Path.Combine(directory, file);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                await File.WriteAllBytesAsync(dest, await response.Content.ReadAsByteArrayAsync());
        });
        await Task.WhenAll(tasks);
    }
}