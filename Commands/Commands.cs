

using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lionpaw{

    namespace Commands{

        //All defualt values are done through here
        [JsonConverter(typeof(CommandConverter))]
        public enum Command{

            [ChoiceName("DB Add")]
            DATA_ADD,
            REMOVE,
            EDIT,
            INCREMENT,
            SHOW_CHARACTER,
            SHOW_ALL,
            SHOW_USER,
            SHOW_FILTERED,
            REGISTER_GUILD,
            MODULE_ENABLE,
            MODULE_DISABLE,
            COMMAND_ENABLE,
            COMMAND_DISABLE,
            COMMAND_EDIT,
            PARAMETER_ADD,
            PARAMETER_REMOVE,
            PARAMETER_EDIT,
            CHANNEL_SETTINGS,
            EightBall,
            ADD

        }

        public enum ModuleEnable{

            [ChoiceName("Weather")]
            WEATHER

        }

        //Handled for when I decide to change values
        #pragma warning disable
        public class CommandConverter : StringEnumConverter{
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer){
                string value = reader.Value.ToString();

                // Handle the conversion from "ADD" to "DATA_ADD"
                if (value == "ADD")
                    value = "DATA_ADD";

                return base.ReadJson(new JsonTextReader(new StringReader(value)), objectType, existingValue, serializer);
            }
        }

    }

}