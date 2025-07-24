using k8s.Models;
using k8s;
using System.Net;

namespace SimulationProject.Helper.KubernetesHelper
{
    public static class RBACHelper
    {
        public static async Task ApplyRbacAsync(IKubernetes client)
        {
            const string namespaceName = "default";
            const string roleName = "master-role";
            const string bindingName = "master-rolebinding";

            // is created if not exists
            try
            {
                await client.RbacAuthorizationV1.ReadNamespacedRoleAsync(roleName, namespaceName);
            }
            catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                var role = new V1Role
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = roleName,
                        NamespaceProperty = namespaceName
                    },
                    Rules = new List<V1PolicyRule>
                {
                    new V1PolicyRule
                    {
                        ApiGroups = new List<string> { "" },
                        Resources = new List<string> { "pods" },
                        Verbs = new List<string> { "get", "list", "create", "watch" }
                    }
                }
                };

                await client.RbacAuthorizationV1.CreateNamespacedRoleAsync(role, namespaceName);
            }

            // is created if not exists
            try
            {
                await client.RbacAuthorizationV1.ReadNamespacedRoleBindingAsync(bindingName, namespaceName);
            }
            catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                var roleBinding = new V1RoleBinding
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = bindingName,
                        NamespaceProperty = namespaceName
                    },
                    Subjects = new List<Rbacv1Subject>
                {
                    new Rbacv1Subject
                    {
                        Kind = "ServiceAccount",
                        Name = "default",
                        NamespaceProperty = namespaceName
                    }
                },
                    RoleRef = new V1RoleRef
                    {
                        Kind = "Role",
                        Name = roleName,
                        ApiGroup = "rbac.authorization.k8s.io"
                    }
                };

                await client.RbacAuthorizationV1.CreateNamespacedRoleBindingAsync(roleBinding, namespaceName);
            }
        }
    }
}
