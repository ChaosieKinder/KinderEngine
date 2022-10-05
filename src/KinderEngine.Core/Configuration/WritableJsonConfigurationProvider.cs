using Microsoft.Extensions.Configuration.Json;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;

namespace KinderEngine.Core.Configuration
{
    public class WritableJsonConfigurationProvider : JsonConfigurationProvider
    {
        public WritableJsonConfigurationProvider(JsonConfigurationSource source) : base(source)
        {
        }

        /// <summary>
        /// Sets the key to the given value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override void Set(string key, string value)
        {
            base.Set(key, value.ToString());

            var fileFullPath = base.Source.FileProvider.GetFileInfo(base.Source.Path).PhysicalPath;
            var jsonObj = JsonObject.Parse(
                File.ReadAllText(fileFullPath));

            _writeValueToDepth(jsonObj, key.Split(":"), 0, value);

            File.WriteAllText(fileFullPath, jsonObj.ToJsonString());
        }

        private void _writeValueToDepth(JsonNode jnode, string[] keys, int currentDepth, string value)
        {
            if(currentDepth == keys.Length)
            {
                // we found a node! now we need to write our value
                if (jnode is JsonObject jobj)
                {
                    if (jobj[keys[currentDepth]] is null)
                    {
                        jobj[keys[currentDepth]] = JsonValue.Create("");
                    }
                    else
                    {
                        var jval = jobj[keys[currentDepth]].AsValue();
                        if(jval.TryGetValue(out int ivalint))
                        {

                            jobj[keys[currentDepth]] = JsonValue.Create(ivalint);
                        }
                        else if (jval.TryGetValue(out string ivalstring))
                        {
                            jobj[keys[currentDepth]] = JsonValue.Create(ivalstring);
                        }
                        else if (jval.TryGetValue(out double ivaldouble))
                        {
                            jobj[keys[currentDepth]] = JsonValue.Create(ivaldouble);
                        }
                        else if (jval.TryGetValue(out bool ivalbool))
                        {
                            jobj[keys[currentDepth]] = JsonValue.Create(ivalbool);
                        }
                        else
                        {
                            Debugger.Break();
                        }
                    }
                }
                else 
                    Debugger.Break();
            }
            else if(jnode is JsonObject jobj)
            {
                if (jobj[keys[currentDepth]] is null) 
                    jobj[keys[currentDepth]] = new JsonObject();
                _writeValueToDepth(jobj[keys[currentDepth]], keys, currentDepth + 1, value);
            }
            else if(jnode is JsonArray jarr
                && int.TryParse(keys[currentDepth], out int jarrIndex))
            {
                if (jarr[jarrIndex] is null)
                    jarr[jarrIndex] = new JsonObject();
                _writeValueToDepth(jarr[jarrIndex], keys, currentDepth + 1, value);
            }
            else
            {
                Debugger.Break();
            }
        }
    }
}
