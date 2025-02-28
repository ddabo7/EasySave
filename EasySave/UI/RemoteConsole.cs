using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using EasySave.Core;
using EasySave.Models;
using System.Linq;

namespace EasySave.UI
{
    public class RemoteConsole
    {
        private readonly TcpListener _listener;
        private readonly BackupManager _backupManager;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<TcpClient> _clients;
        private readonly object _clientsLock = new object();

        public RemoteConsole(BackupManager backupManager, int port = 5001)
        {
            _backupManager = backupManager;
            _listener = new TcpListener(IPAddress.Any, port);
            _cancellationTokenSource = new CancellationTokenSource();
            _clients = new List<TcpClient>();

            // S'abonner aux événements du BackupManager
            _backupManager.OnProgress += (jobName, progress) => BroadcastToClients($"Progress:{jobName}:{progress}");
            _backupManager.OnBackupComplete += (jobName, success, error) => 
                BroadcastToClients($"Complete:{jobName}:{success}:{error ?? "null"}");
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("Remote console started on port 5001");

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (OperationCanceledException)
            {
                // Arrêt normal
            }
            finally
            {
                _listener.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"New client connected from {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            
            lock (_clientsLock)
            {
                _clients.Add(client);
                Console.WriteLine($"Total clients connected: {_clients.Count}");
            }

            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true };

                Console.WriteLine("Sending welcome message...");
                await writer.WriteLineAsync("Bienvenue sur la Console Déportée EasySave");
                await writer.WriteLineAsync("----------------------------------------");
                await writer.WriteLineAsync("Commandes disponibles :");
                await writer.WriteLineAsync("  list   : Affiche la liste des jobs de sauvegarde");
                await writer.WriteLineAsync("  status : Affiche l'état des jobs en cours");
                await writer.WriteLineAsync("  start <nom_job> : Démarre un job de sauvegarde");
                await writer.WriteLineAsync("  stop <nom_job>  : Arrête un job de sauvegarde");
                await writer.WriteLineAsync("  help   : Affiche ce message d'aide");
                await writer.WriteLineAsync("----------------------------------------");
                Console.WriteLine("Welcome message sent");

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var command = await reader.ReadLineAsync();
                    Console.WriteLine($"Received command: {command}");
                    
                    if (string.IsNullOrEmpty(command)) 
                    {
                        Console.WriteLine("Client disconnected (empty command)");
                        break;
                    }

                    var response = await ProcessCommandAsync(command);
                    Console.WriteLine($"Sending response: {response}");
                    await writer.WriteLineAsync(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                lock (_clientsLock)
                {
                    _clients.Remove(client);
                    Console.WriteLine($"Client disconnected. Total clients remaining: {_clients.Count}");
                    client.Dispose();
                }
            }
        }

        private async Task<string> ProcessCommandAsync(string command)
        {
            var parts = command.Split(' ');
            if (parts.Length == 0) return "Invalid command";

            switch (parts[0].ToLower())
            {
                case "start":
                    if (parts.Length < 2) return "Usage: start <jobName>";
                    await _backupManager.ExecuteJobs(new[] { parts[1] });
                    return "Backup started";

                case "stop":
                    if (parts.Length < 2) return "Usage: stop <jobName>";
                    _backupManager.StopJob(parts[1]);
                    return "Backup stopped";

                case "list":
                    return GetJobsList();

                case "status":
                    return GetJobsStatus();

                case "help":
                    return "Commandes disponibles :\n" +
                           "  list   : Affiche la liste des jobs de sauvegarde\n" +
                           "  status : Affiche l'état des jobs en cours\n" +
                           "  start <nom_job> : Démarre un job de sauvegarde\n" +
                           "  stop <nom_job>  : Arrête un job de sauvegarde\n" +
                           "  help   : Affiche ce message d'aide";

                default:
                    return "Unknown command. Available commands: start, stop, list, status, help";
            }
        }

        private void BroadcastToClients(string message)
        {
            lock (_clientsLock)
            {
                foreach (var client in _clients.ToList())
                {
                    try
                    {
                        var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                        writer.WriteLine(message);
                    }
                    catch
                    {
                        _clients.Remove(client);
                        client.Dispose();
                    }
                }
            }
        }

        private string GetJobsList()
        {
            var jobs = _backupManager.BackupJobs;
            if (!jobs.Any()) return "No backup jobs configured";
            
            var jobsList = new StringBuilder();
            jobsList.AppendLine("Liste des jobs de sauvegarde :");
            jobsList.AppendLine("----------------------------");
            
            foreach (var job in jobs)
            {
                jobsList.AppendLine($"Nom: {job.Name}");
                jobsList.AppendLine($"Type: {job.Type}");
                jobsList.AppendLine($"Source: {job.SourcePath}");
                jobsList.AppendLine($"Destination: {job.DestinationPath}");
                jobsList.AppendLine($"Statut: {job.Status}");
                jobsList.AppendLine("----------------------------");
            }
            
            return jobsList.ToString();
        }

        private string GetJobsStatus()
        {
            var jobs = _backupManager.BackupJobs;
            if (!jobs.Any()) return "Aucun job configuré";
            
            var status = new StringBuilder();
            status.AppendLine("État des jobs de sauvegarde :");
            status.AppendLine("----------------------------");
            
            foreach (var job in jobs)
            {
                status.AppendLine($"Job: {job.Name}");
                status.AppendLine($"Statut: {job.Status}");
                status.AppendLine($"Progression: {job.Progress}%");
                status.AppendLine($"Taille: {job.TotalFileSize} octets");
                status.AppendLine("----------------------------");
            }
            
            return status.ToString();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            lock (_clientsLock)
            {
                foreach (var client in _clients)
                {
                    client.Dispose();
                }
                _clients.Clear();
            }
        }
    }
}
