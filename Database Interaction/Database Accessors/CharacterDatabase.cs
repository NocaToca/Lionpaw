using Lionpaw.Characters;

namespace Lionpaw{

    namespace Databases{

        public static class CharacterAccessor{

            public async static Task<Character> GrabCharacter(string name, ulong user_id, ulong guild_id){
                List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(guild_id));

                foreach(Character character in characters){
                    if(character.name == name && character.user_id == user_id){
                        return character;
                    }
                }

                throw new Exception();

            }

        }
    }

}