namespace WarpWorld.CrowdControl
{
    public class CCEffectInstanceParameters : CCEffectInstance
    {
        /// <summary>The parameters sent into the effect. Eg: Item Type, Quantity</summary>
        public void AssignParameters(string newParams)
        {
            Parameters = newParams.Substring(1, newParams.Length - 2).Split(',');
        }

        /// <summary>Get the parameter based on it's specific index.</summary>
        public string GetParameter(int index)
        {
            if (Parameters == null || Parameters.Length == 0 || index >= Parameters.Length)
            {
                return string.Empty;
            }

            string param = Parameters[index];

            if (param[0] == '[' && param[param.Length - 1] == ']')
            {
                param = param.Substring(1, param.Length - 2);
            }

            return param;
        }
    }
}
