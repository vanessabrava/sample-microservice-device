namespace Sample.Microservice.Device.Infra.CrossCutting.Options
{
    public class GlobalizationOptions
    {
        public string DefaultRequestCulture { get; set; }

        public string[] SupportedCultures { get; set; }

        public string[] SupportedUICultures { get; set; }

        public string FormatDecimal { get; set; }

        public int DecimalPlaces { get; set; }

        public string DateFormat { get; set; }
    }
}
