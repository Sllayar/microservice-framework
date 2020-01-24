using System;
using Prometheus;

// ReSharper disable All

namespace RFI.MicroserviceFramework._Metrics
{
    public class Metrica
    {
        public Metrica(string name, params string[] labelNames)
        {
            counter = Metrics.CreateCounter(name, "", new CounterConfiguration { LabelNames = labelNames });
            counter.Inc(0);
        }

        private readonly Counter counter;

        public void Inc(params object[] values) => Inc(1, values);

        public void Inc(double increment, params object[] labels)
        {
            try
            {
                var values = new string[labels.Length];

                for(var i = 0; i < labels.Length; i++)
                {
                    var label = labels[i];
                    switch(label)
                    {
                        case bool labelBool:
                            values[i] = labelBool ? Status.OK : Status.FAIL;
                            continue;

                        case Enum labelEnum:
                            values[i] = labelEnum.ToString("G");
                            continue;

                        default:
                            values[i] = label.ToString();
                            continue;
                    }
                }

                counter.WithLabels(values).Inc(increment);
            }
            catch(Exception)
            {
                // do nothing
            }
        }
    }
}