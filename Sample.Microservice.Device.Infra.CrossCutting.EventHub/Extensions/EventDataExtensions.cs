using Microsoft.Azure.EventHubs;
using System.Collections.Generic;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Extensions
{
    public static class EventDataExtensions
    {
        private static string IsOkProperty => "IS_OK";

        private static string SendToShuntProperty => "SEND_TO_SHUNT";

        private static string RetryProperty => "NUMBER_OF_RETRY";

        public static EventData AddSendToShuntProperty(this EventData eventData)
        {
            eventData.RemoveSendToShuntProperty();

            eventData.Properties.Add(SendToShuntProperty, true);

            return eventData;
        }

        public static EventData RemoveSendToShuntProperty(this EventData eventData)
        {
            if (eventData.HasSendToShuntProperty()) eventData.Properties.Remove(SendToShuntProperty);

            return eventData;
        }

        public static bool HasSendToShuntProperty(this EventData eventData) => eventData.Properties.ContainsKey(SendToShuntProperty);

        public static EventData AddIsOkProperty(this EventData eventData)
        {
            eventData.RemoveIsOkProperty();

            eventData.Properties.Add(IsOkProperty, true);

            return eventData;
        }

        public static EventData RemoveIsOkProperty(this EventData eventData)
        {
            if (eventData.HasIsOkProperty()) eventData.Properties.Remove(IsOkProperty);

            return eventData;
        }

        public static bool HasIsOkProperty(this EventData eventData) => eventData.Properties.ContainsKey(IsOkProperty);

        public static EventData AddIfNotExistsRetryProperty(this EventData eventData)
        {
            if (!eventData.HasRetryProperty()) eventData.Properties.Add(RetryProperty, 0);

            return eventData;
        }

        public static EventData RemoveRetryProperty(this EventData eventData)
        {
            if (eventData.HasRetryProperty()) eventData.Properties.Remove(RetryProperty);

            return eventData;
        }

        public static int IncrementRetryProperty(this EventData eventData)
        {
            eventData.AddIfNotExistsRetryProperty();
            int incrementedValue = (int)eventData.Properties[RetryProperty] + 1;
            eventData.Properties[RetryProperty] = incrementedValue;
            return incrementedValue;
        }

        public static bool HasRetryProperty(this EventData eventData) => eventData.Properties.ContainsKey(RetryProperty);

        public static int CurrentRetryPropertyValue(this EventData eventData)
        {
            if (!eventData.HasRetryProperty()) return -1;

            return (int)eventData.Properties[RetryProperty];
        }
    }
}