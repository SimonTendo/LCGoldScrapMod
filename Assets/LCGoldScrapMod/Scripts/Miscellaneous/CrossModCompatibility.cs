using UnityEngine;

public class CrossModCompatibility
{
    public static void CreateGoldCrate()
    {
        CrashBandicootCratesAllAssets allAssets = CrashBandicootCratesManagerScript.allAssets;

        Item originalWumpa = null;
        Mesh originalWumpaMesh = null;

        for (int i = 0; i < allAssets.allItems.Count; i++)
        {
            Item thisItem = allAssets.allItems[i];
            if (thisItem != null && thisItem.itemName == "Wumpa Fruit")
            {
                originalWumpa = thisItem;
                break;
            }
        }

        if (originalWumpa != null && originalWumpa.spawnPrefab != null)
        {
            MeshFilter wumpaRenderer = originalWumpa.spawnPrefab.GetComponent<MeshFilter>();
            if (wumpaRenderer != null && wumpaRenderer.mesh != null)
            {
                originalWumpaMesh = wumpaRenderer.mesh;
            }
        }

        GameObject goldWumpa = GoldScrapPlugin.CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/CrossModCompatibility/LCCrashBandicootCratesMod/GoldWumpaPrefab.prefab");
        goldWumpa.GetComponent<MeshFilter>().mesh = originalWumpaMesh;

        WumpaFruitProp itemScript = goldWumpa.AddComponent<WumpaFruitProp>();
        itemScript.grabbable = true;
        itemScript.stackOf = 1;
        itemScript.scanNode = goldWumpa.GetComponentInChildren<ScanNodeProperties>();
        itemScript.itemProperties = GoldScrapPlugin.CustomGoldScrapAssets.LoadAsset<Item>("Assets/LCGoldScrapMod/CrossModCompatibility/LCCrashBandicootCratesMod/GoldWumpaItem.asset");
        itemScript.itemProperties.grabSFX = originalWumpa.grabSFX;
        itemScript.itemProperties.maxValue = (int)(originalWumpa.maxValue * GoldScrapConfigs.maxValueMultiplier.Value);
        itemScript.itemProperties.minValue = (int)(originalWumpa.minValue * GoldScrapConfigs.minValueMultiplier.Value);

        Material[] switchMatsTo = new Material[51];
        switchMatsTo[50] = allAssets.allCrateMaterials[0];
        int[] changeMatIndeces = { 1, 2 };

        Item goldCrateItemProperties = CreateACrate.MakeItem(
            crateName: "Gold Crate",
            twoHanded: true,
            twoHandedAnim: true,
            disableWallHands: false,
            grabBeforeStart: true,
            weight: 1.715f,
            trigger: false,
            holdButtonUse: false,
            spawnsOnGround: true,
            conductive: true,
            maxValue: (int)(625 * GoldScrapConfigs.maxValueMultiplier.Value),
            minValue: (int)(300 * GoldScrapConfigs.minValueMultiplier.Value),
            requireBattery: false,
            batteryUsage: 0.0f,
            autoUsePower: false,
            crateIcon: GoldScrapPlugin.CustomGoldScrapAssets.LoadAsset<Sprite>("Assets/LCGoldScrapMod/CrossModCompatibility/LCCrashBandicootCratesMod/GoldCrateIcon.png"),
            grabAnim: "HoldLung",
            grabSFX: null,
            dropSFX: null,
            pocketSFX: null,
            defensiveWeapon: false,
            tooltips: new string[2] { "Thank you for playing:", "LCGoldScrapMod" },
            verticalOffset: 0.01f,
            floorOffset: 0,
            dropAhead: true,
            restRotation: default,
            rotationOffset: default,
            positionOffset: default,
            meshVariants: null,
            materialVariants: null,
            usableInAnimations: false,
            inspectable: false
            );

        CrateData goldCrateData = CreateACrate.MakeCrateData(
            breakSFX: null,
            bounceSFX: null,
            specialSFX: new AudioClip[1] { allAssets.allAudioClips[4] },
            audioOneShot: true,
            audioToggle: true,
            audioRandomized: false,
            noiseAudible: true,
            noiseLoudness: 1.0f,
            noiseRange: 25.0f,
            noiseInterval: 2.0f,
            switchMats: switchMatsTo,
            matIndeces: changeMatIndeces,
            bounceThreshold: 2.5f,
            bounceDivider: 10.0f,
            bounceTime: 2.5f,
            bounceCurve: null,
            fallTriggersWalk: -1.0f,
            fallTriggersJump: -1.0f,
            fallTriggersAttack: 2.5f,
            walkPlaySFX: false,
            jumpPlaySFX: false,
            attackPlaySFX: true,
            timerPlaySFX: false,
            destroyPlaySFX: false,
            walkPlayParticle: false,
            jumpPlayParticle: false,
            attackPlayParticle: true,
            timerPlayParticle: false,
            destroyPlayParticle: false,
            switchPrefabsOnInt: null,
            switchToNull: false,
            countInBetween: true,
            valueDecrease: 1.0f,
            walkSpawnAmount: -1,
            jumpSpawnAmount: 1,
            attackSpawnAmount: 10,
            destroySpawnAmount: -1,
            walkLosesContent: false,
            jumpLosesContent: false,
            attackLosesContent: false,
            otherLosesContent: false,
            timerRepeats: 0,
            timerVisible: true,
            timerMedian: 45.0f
            );

        ModdedCrateRuntimeParameters goldCrateRuntimeParameters = CreateACrate.MakeModdedCrateRuntimeParameters(
            crateData: goldCrateData,
            itemProperties: goldCrateItemProperties,
            scanName: null,
            crateScale: new Vector3(1.05f, 1.05f, 1.05f),
            newMaterials: new Material[3] { GoldScrapAssetsCollection.defaultMaterialGold, GoldScrapAssetsCollection.defaultMaterialGold, GoldScrapAssetsCollection.defaultMaterialGold },
            newMesh: null,
            newBreakParticles: null,
            breakParticleColor: Color.yellow,
            specialParticles: allAssets.allParticles[1],
            specialParticleColor: Color.yellow,
            collide: true,
            overrideCollide: null,
            overrideBounce: null,
            useCooldown: 0.5f,
            bounceForce: 20.0f,
            startingInt: 0,
            destroyUpon: -1,
            addOrSnap: true,
            walkChangesInt: 0,
            jumpChangesInt: 1,
            attackChangesInt: 10,
            startEvent: null,
            walkIntoEvent: null,
            jumpOntoEvent: null,
            attackEvent: null,
            timerEndEvent: null,
            destroyEvent: null,
            newSpawnPrefab: goldWumpa,
            spawnAmount: 50,
            timerDuration: -1.0f,
            timerObject: null
            );


        CreateACrate.RegisterCrateToAllLevelsWithRarity(
            runtimeParameters: goldCrateRuntimeParameters,
            rarity: 1
            //rarity: Mathf.Max(1, (int)(34f * GoldScrapConfigs.rarityMultiplier.Value))
            );

        GameObject projectilePrefab = GoldScrapPlugin.CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/CrossModCompatibility/LCCrashBandicootCratesMod/BazookaProjectileGoldWumpa.prefab");
        if (projectilePrefab == null)
        {
            GoldScrapPlugin.Logger.LogError("Failed to load projectilePrefab for LCCrashBandicootCratesMod compatibility!");
            return;
        }
        projectilePrefab.GetComponentInChildren<MeshFilter>().mesh = originalWumpaMesh;

        Projectile originalWumpaProjectile = null;
        if (allAssets != null && allAssets.allProjectiles != null && allAssets.allProjectiles.Count > 0 && allAssets.allProjectiles[0] != null)
        {
            originalWumpaProjectile = allAssets.allProjectiles[0].GetComponent<Projectile>();
        }

        ProjectileWumpa goldWumpaProjectile = projectilePrefab.AddComponent<ProjectileWumpa>();
        goldWumpaProjectile.hitPower = 2;
        goldWumpaProjectile.travelingSpeed = 20.0f;
        goldWumpaProjectile.lifetime = 5.0f;
        goldWumpaProjectile.spinDirection = new Vector3(2.0f, 0.0f, 0.0f);
        goldWumpaProjectile.spawnOnDestroy = originalWumpaProjectile != null ? originalWumpaProjectile.spawnOnDestroy : GoldScrapPlugin.CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/CrossModCompatibility/LCCrashBandicootCratesMod/GoldWumpaProjectileSplatters.prefab");
        goldWumpaProjectile.spawnedObjectLifetime = 10.0f;
        goldWumpaProjectile.audioSource = goldWumpaProjectile.GetComponentInChildren<AudioSource>();
        goldWumpaProjectile.fireSFX = originalWumpaProjectile != null ? originalWumpaProjectile.fireSFX : null;
        goldWumpaProjectile.hitSFX = originalWumpaProjectile != null ? originalWumpaProjectile.hitSFX : null;
        goldWumpaProjectile.noiseAudible = true;
        goldWumpaProjectile.noiseRange = 15.0f;
        goldWumpaProjectile.noiseLoudness = 0.75f;
        goldWumpaProjectile.splatterMat = GoldScrapPlugin.CustomGoldScrapAssets.LoadAsset<Material>("Assets/LCGoldScrapMod/CrossModCompatibility/LCCrashBandicootCratesMod/GoldWumpaSplatterMat.mat");

        CreateACrate.RegisterProjectile(
            projectileScript: goldWumpaProjectile,
            ammoItem: itemScript
            );
    }
}
