using System;
using System.IO;
using RFI.MicroserviceFramework._Helpers;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Kubernetes;

namespace RFI.MicroserviceFramework._Environment
{
    public static class Vault
    {
        public static void Init()
        {
            LoadToEnvironment(
                SEnv.Get("VAULT_KUBER_JWT_PATH"),
                SEnv.Get("VAULT_SERVICE_URL"),
                SEnv.Get("VAULT_MOUNT_POINT"),
                SEnv.Get("VAULT_PATH"),
                SEnv.Get("VAULT_ROLE")
            );
        }

        private static void LoadToEnvironment(string kuberJwtPath, string serviceUrl, string mountPoint, string path, string role)
        {
            if(SEnv.Get("VaultInited").NotEmpty()) return;

            var vaultClientSettings = new VaultClientSettings(serviceUrl, new KubernetesAuthMethodInfo(mountPoint, role, File.ReadAllText(kuberJwtPath)));
            vaultClientSettings.PostProcessHttpClientHandlerAction += handler => handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            IVaultClient vaultClient = new VaultClient(vaultClientSettings);

            var secrets = vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync(path).Result.Data;

            foreach(var (key, value) in secrets) Environment.SetEnvironmentVariable(key, value.ToString());

            Environment.SetEnvironmentVariable("VaultInited", "1");
        }
    }
}