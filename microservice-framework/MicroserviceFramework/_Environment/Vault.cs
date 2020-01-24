using System;
using System.IO;
using RFI.MicroserviceFramework._Helpers;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.AuthMethods.LDAP;

// ReSharper disable All

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
                SEnv.Get("VAULT_ROLE"),
                SEnv.Get("VAULT_LDAP_USER").Base64Decode(),
                SEnv.Get("VAULT_LDAP_PASS").Base64Decode()
            );
        }

        private static void LoadToEnvironment(string kuberJwtPath, string serviceUrl, string mountPoint, string path, string role, string ldapUser, string ldapPass)
        {
            if(SEnv.Get("VaultInited").NotEmpty()) return;

            var authMethod = SEnv.IsDebug ? new LDAPAuthMethodInfo(ldapUser, ldapPass) as IAuthMethodInfo : new KubernetesAuthMethodInfo(mountPoint, role, File.ReadAllText(kuberJwtPath)) as IAuthMethodInfo;

            var vaultClientSettings = new VaultClientSettings(serviceUrl, authMethod);
            vaultClientSettings.PostProcessHttpClientHandlerAction += handler => handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            IVaultClient vaultClient = new VaultClient(vaultClientSettings);

            var secrets = vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync(path).Result.Data;

            foreach(var (key, value) in secrets) Environment.SetEnvironmentVariable(key, value.ToString());

            Environment.SetEnvironmentVariable("VaultInited", "1");
        }
    }
}