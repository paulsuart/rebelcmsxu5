﻿using System.Linq;
using RebelCms.Framework.Localization.Parsing;
using RebelCms.Framework.Localization.Support;

namespace RebelCms.Framework.Localization.Processing.ParameterEvaluators
{
    public class ReflectionParameterEvaluator : IParameterEvaluator
    {
        public string BaseParameterName { get; private set; }
        public string[] Properties { get; set; }

        public ReflectionParameterEvaluator(string parameterName, string[] properties)
        {
            BaseParameterName = parameterName;
            Properties = properties;
        }

        public ParameterValue GetValue(EvaluationContext context)
        {
            object val = context.Parameters.GetObject(BaseParameterName);
            if (val != null)
            {
                foreach (var prop in Properties)
                {
                    val = ObjectHelper.Eval(val, prop);
                }
            }
            return ParameterValue.Wrap(val);
        }
    }

    public class ReflectionParameterEvaluatorFactory : IParameterEvaluatorFactory
    {

        public IParameterEvaluator GetFor(ParameterSpec spec, PatternDialect dialect, TextManager manager)
        {
            var parts = spec.ParameterName.Split('.');
            if (parts.Length > 1)
            {
                return new ReflectionParameterEvaluator(parts[0], parts.Skip(1).ToArray());
            }

            return null;
        }
    }
}