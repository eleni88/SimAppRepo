using System.Collections.Generic;
using System.IO;
using k8s;
using k8s.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimulationProject.Helper.KubernetesHelper
{
    public static class KubernetesYamlHelper
    {
        public static List<IKubernetesObject> LoadAllObjects(string yaml)
        {
            var resources = new List<IKubernetesObject>();

            foreach (var obj in KubernetesYaml.LoadAllFromString(yaml))
            {
                if (obj is IKubernetesObject k8sObj)
                    resources.Add(k8sObj);
            }

            return resources;
        }
    }
}
