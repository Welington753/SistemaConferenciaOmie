using System;
using System.IO;
using System.Linq;
using SistemaConferenciaPedidos.Data;

namespace SistemaConferenciaPedidos.Services
{
    public class ResultadoBackup
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = "";
        public Exception? ErroTecnico { get; set; }
    }

    public class BackupBancoService
    {
        private readonly int _maxBackups;

        public BackupBancoService(int maxBackups = 20)
        {
            _maxBackups = maxBackups;
        }

        public ResultadoBackup RealizarBackup()
        {
            try
            {
                string caminhoBanco = Database.CaminhoBanco;
                
                if (!File.Exists(caminhoBanco))
                {
                    return new ResultadoBackup { Sucesso = false, Mensagem = "Arquivo do banco de dados não encontrado para realizar o backup." };
                }

                string pastaDados = Path.GetDirectoryName(caminhoBanco);
                string pastaBackups = Path.Combine(pastaDados, "Backups");

                if (!Directory.Exists(pastaBackups))
                {
                    Directory.CreateDirectory(pastaBackups);
                }

                string dataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string nomeArquivoBackup = $"sistema_conferencia_{dataHora}.db";
                string caminhoDestino = Path.Combine(pastaBackups, nomeArquivoBackup);

                File.Copy(caminhoBanco, caminhoDestino, true);

                LimparBackupsAntigos(pastaBackups);

                return new ResultadoBackup { Sucesso = true, Mensagem = "Backup realizado com sucesso." };
            }
            catch (Exception ex)
            {
                return new ResultadoBackup
                {
                    Sucesso = false,
                    Mensagem = "Erro ao realizar backup do banco de dados.",
                    ErroTecnico = ex
                };
            }
        }

        private void LimparBackupsAntigos(string pastaBackups)
        {
            var arquivosBackup = Directory.GetFiles(pastaBackups, "sistema_conferencia_*.db")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            if (arquivosBackup.Count > _maxBackups)
            {
                var backupsParaRemover = arquivosBackup.Skip(_maxBackups).ToList();
                foreach (var arquivo in backupsParaRemover)
                {
                    try
                    {
                        arquivo.Delete();
                    }
                    catch
                    {
                        // Se falhar ao deletar arquivo antigo, ignora para não quebrar a execução principal
                    }
                }
            }
        }
    }
}
