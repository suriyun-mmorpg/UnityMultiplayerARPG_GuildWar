using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO.GuildWar
{
    public class GWManager : MonoBehaviour
    {
        private static GWManager singleton;
        public static GWManager Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new GameObject("_GWManager").AddComponent<GWManager>();
                    DontDestroyOnLoad(singleton.gameObject);
                }
                return singleton;
            }
        }

        public int DefenderGuildId = 0;
    }
}
