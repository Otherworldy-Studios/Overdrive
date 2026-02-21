using System;
using UnityEngine;

[Serializable]
public class RewardsPacket
{
   private int arcaneCredit = 0;
   private Placeholder.Upgrade upgrade;
   private bool healthRestore;
   private Placeholder.Player player;

   public RewardsPacket(Placeholder.Player player, int arcaneCredit, bool shouldRestoreHealth, Placeholder.Upgrade upgrade)
   {
      this.player = player;
      this.arcaneCredit = arcaneCredit;
      this.healthRestore = shouldRestoreHealth;
      this.upgrade = upgrade;
   }

   public RewardsPacket(Placeholder.Player player, int arcaneCredit, bool shouldRestoreHealth)
   {
      this.player = player;
      this.arcaneCredit = arcaneCredit;
      this.healthRestore = shouldRestoreHealth;
   }

   public RewardsPacket(Placeholder.Player player, bool shouldRestoreHealth)
   {
      this.player = player;
      this.healthRestore = shouldRestoreHealth;
   }
   
   public RewardsPacket(int arcaneCredit)
   {
      this.arcaneCredit = arcaneCredit;
   }

   public void GiveRewards()
   {
      Placeholder.CreditManager.AddCredit(arcaneCredit);
      Placeholder.Upgrade.AddUpgrade();
      if (healthRestore)
      {
         player.RestoreToFull();
      }
   }
   

   

}

namespace Placeholder
{
   public class Upgrade
   {
      public static void AddUpgrade()
      {
         
      }
   }

   public static class CreditManager
   {
      public static void AddCredit(int amount)
      {
         
      }
   }

   public class Player
   {
      public void RestoreToFull()
      {
         
      }
   }
}