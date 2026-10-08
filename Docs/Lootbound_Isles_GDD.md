# Lootbound Isles — Game Design Document

> Source of truth for target gameplay and functionality.
> Generated from the approved GDD. Exact balance values remain configurable unless explicitly fixed.

Lootbound Isles

Game Design Document — зафиксированная целевая логика игры, версия 1.0

Дата фиксации: 01.10.2026

Платформа разработки: Unity. Целевая публикация: Яндекс Игры.

Документ описывает согласованную игровую логику и функциональность. Точные числовые значения баланса — проценты, формулы, цены, размеры AoE, конкретные LootTable и характеристики зон — настраиваются отдельно и не считаются окончательно зафиксированными, если это прямо не указано.

# 1. Видение игры

Lootbound Isles — single-player Action / Idle RPG с постоянным персонажем и единым непрерывным миром. Игра не строится вокруг отдельных roguelite-забегов: прогресс персонажа, экипировка, ресурсы, открытые зоны и мета-прогрессия сохраняются.

Исследование мира  →  Убийство монстров  →  EXP и loot  →  Level Up  →  Улучшение экипировки  →  Boss  →  Новая зона  →  Rebirth  →  Повторное развитие

- Основная активная игра — перемещение по 3D-миру и автоматический бой с ближайшими противниками.

- Основная долгосрочная сила — Equipment, Equipment Level, Efficiency, Gems, Rebirth Power и ресурсы.

- Level Up с выбором 1 из 3 усилений остаётся важной, но сбрасываемой при Rebirth прогрессией.

- Idle-часть представлена Offline Rewards, но не заменяет активный фарм ценной добычи.

- На первом этапе gameplay создаётся на стандартных примитивах Unity; модели и визуалы заменяются позже без изменения gameplay logic.

# 2. Архитектурные принципы

- Data-driven подход: характеристики врагов, оружия, предметов, спавнеров, loot, зон, ритуалов и наград задаются данными.

- Gameplay logic не зависит от цвета, размера, конкретного prefab или модели.

- Данные, gameplay logic, presentation и UI разделены.

- Не использовать монолитный GameManager и огромные MonoBehaviour-классы.

- Системы должны быть простыми и расширяемыми для одного разработчика.

- Каждый этап разработки заканчивается рабочим и проверяемым состоянием проекта.

# 3. Мир и зоны

## 3.1. Общая структура

Игра использует один большой непрерывный мир. Safe Zone, боевые зоны, Dungeon и Ritual Zone являются частями одного мира. Обычные переходы не требуют загрузки отдельной сцены. Технический streaming в будущем допустим, но не меняет игровую модель.

- Safe Zone — центральный безопасный хаб с NPC и сервисами.

- Zone 1 доступна с начала новой игры.

- Следующие зоны открываются навсегда после первого убийства Main Boss предыдущей зоны.

- Нет требований Player Level, Gold или ресурсов для базового открытия следующей зоны.

- После последней созданной зоны игрок переходит в endgame farming; новые зоны добавляются обновлениями.

- Старая концепция бесконечного WorldLevel и автоматического World Scaling отменена.

## 3.2. Сложность зон

Характеристики врагов определяются конкретным контентом зоны и EnemyDefinition. Они не масштабируются автоматически от Player Level или Rebirth Rank.

- HP

- Damage

- Defense

- Accuracy

- Evasion

- EXP Reward

- LootTable

Возвращение сильного персонажа в раннюю зону должно ощущаться как реальный рост силы: старые противники не подстраиваются под игрока.

## 3.3. Safe Zone

- В Safe Zone не появляются обычные монстры.

- Здесь находятся Blacksmith, Merchant/Buyer, Healer, Crafter, Jeweler, Blueprint NPC и другие сервисные NPC.

- Сервисные окна NPC нельзя открыть, пока Player находится In Combat.

- Открытие NPC-окон не ставит игру на паузу.

## 3.4. Dungeon

Dungeon — не отдельная сцена, не инстанс и не run. Это специальная высокоуровневая зона внутри общего мира.

- Доступ к конкретному Dungeon открывается навсегда после заданного milestone мира.

- После открытия вход бесплатный: Keys/Tickets не нужны.

- Обычные Dungeon-враги используют свои spawn cycles.

- Dungeon Boss имеет real-time respawn cooldown, продолжающийся при закрытой игре.

- Игрок может идти прямо к Dungeon Boss; обязательного kill-counter или room-run нет.

- Смерть в Dungeon использует глобальную мягкую смерть: Player возвращается в общую Safe Zone.

## 3.5. Ritual Zone

Ritual Zone физически существует в мире с начала игры. Игрок может найти её заранее, но активировать ритуал нельзя до выполнения требований текущего Rebirth.

# 4. Карта и навигация

## 4.1. Mini Map

- Показывает Player.

- Показывает Safe Zone и важные NPC/ориентиры.

- Показывает Dungeon, обнаруженную Ritual Zone, Main Boss и важные переходы/границы зон.

- Обычные враги на Mini Map не отображаются.

## 4.2. World Map

Отдельное окно большой карты используется для навигации по единому миру.

- Использует Fog of War / Map Discovery.

- Неисследованные территории скрыты или затемнены.

- Физически посещённые участки открываются навсегда.

- Safe Zone известна с начала.

- Открытие карты сохраняется и не сбрасывается при Rebirth.

- Dungeon и Ritual Zone после обнаружения остаются отмеченными.

- Fast Travel, Waypoints и Return Stone отсутствуют. Перемещение по миру физическое.

# 5. Player и управление

- PC: WASD.

- Mobile: виртуальный joystick добавляется позднее.

- Атака автоматическая по ближайшему валидному живому противнику в радиусе текущего оружия.

- Ручных боевых skill-кнопок у Player нет.

- Player может менять Weapon/Equipment в любой момент, включая Combat.

- Inventory доступен во время Combat и не ставит игру на паузу.

# 6. Combat System

## 6.1. Общая последовательность атаки

Attack  →  Hit Roll (Accuracy/Evasion)  →  Miss = 0 или Hit  →  Critical Roll  →  Raw Damage  →  Defense  →  Half-up rounding  →  Minimum Damage  →  Final Damage

- Accuracy и Evasion есть у Player и врагов.

- При равных Accuracy/Evasion ориентир hit chance около 90%; точная формула балансируется позже.

- Минимальный hit chance ориентировочно 20%, максимальный — до 100%.

- Critical Damage фиксирован ×2 для всех сторон; Crit Chance может различаться.

- Defense использует числовую формулу с diminishing returns; точная формула позже.

- Успешный Hit после Defense наносит минимум 1 damage.

- HP и итоговый Damage целочисленные; дробные вычисления внутри допускаются; округление .5 — вверх.

## 6.2. Combat State

Combat State влияет прежде всего на Health Regeneration и доступ к NPC-сервисам.

- Любое фактическое Attack Event с участием Player обновляет Combat State, даже если атака промахнулась.

- Player Hit/Miss и Enemy Hit/Miss против Player обновляют таймер.

- Bow firing сразу считается combat event; достижение/impact projectile также обновляет состояние.

- Если Bow projectile исчез из-за смерти цели до impact, нового combat event нет.

- Простое обнаружение, преследование или нахождение рядом с врагом не считается Combat.

- После последнего qualifying event ориентировочно через 5 секунд Player выходит из Combat.

## 6.3. Health Regeneration

- Базовый ориентир — около 1 HP/сек, далее stat развивается.

- Дробная регенерация накапливается внутренне; целые HP применяются, остаток сохраняется.

- При полном HP регенерация не работает, дробный accumulator сбрасывается.

- В Combat регенерация отключена.

- После respawn Combat State сброшен, поэтому регенерация может начаться сразу.

# 7. Weapons

Один слот Weapon. Класса персонажа нет: стиль автоматической атаки определяется текущим Weapon.

При смене оружия текущий attack cooldown сохраняется: swap не позволяет сбросить cooldown и ускорить атаку.

## 7.1. GreatSword

- Направление удара — к выбранной ближайшей цели.

- Фронтальная область/сектор и MaxTargets настраиваются.

- Если целей больше MaxTargets, выбираются ближайшие к Player.

- Один общий Critical Roll на весь swing.

- Hit Roll Accuracy/Evasion выполняется отдельно для каждой цели.

## 7.2. Bow

- Projectile привязан к исходной цели и не ретаргетится.

- Если цель умерла/стала невалидной до достижения, projectile исчезает.

- При выстреле snapshot: Damage, Accuracy, Crit Chance.

- Hit Roll выполняется при достижении живой цели: stored Accuracy против текущего Evasion цели.

- После успешного Hit выполняется Crit Roll с сохранённым Crit Chance, затем Defense.

# 8. Enemy System

## 8.1. Типы

## 8.2. Leash / Returning Home

- У врага фиксируется Home Position.

- Leash Range измеряется от текущей позиции Enemy до Home Position.

- При выходе за Leash враг переходит Returning Home.

- В Returning Home враг прекращает chase/attack, игнорирует Detection и не re-engage при получении damage.

- Враг остаётся уязвимым и может быть убит; уже выпущенные Player projectiles продолжают работать.

- При достижении Home Arrival Radius враг полностью восстанавливает HP и возвращается в Normal AI.

- Anti-stuck основан на отсутствии прогресса/валидного пути. После configurable Stuck Timer враг телепортируется домой, полностью лечится и сбрасывается.

- При Player Death engaged enemies переходят Returning Home.

## 8.3. Special Attacks

- Elite/MiniBoss/Boss могут иметь AI-driven специальные атаки.

- Опасные AoE могут использовать Telegraph: видимая зона предупреждения и задержка.

- Попадание Telegraph AoE определяется в момент impact.

- Если Player вне зоны на impact — 0 damage; внутри — гарантированный Hit.

- Telegraph AoE обходит Accuracy/Evasion и не может Crit.

- Normal Enemy не использует Telegraph AoE, но может быть melee или ranged.

## 8.4. Обычный Enemy Projectile

- Нет отдельной механики физического уклонения траекторией: Miss определяется Accuracy/Evasion.

- При fire сохраняются Hit/Miss, Damage и Crit Chance.

- Изменение Evasion Player после выстрела не меняет уже сохранённый Hit/Miss.

- Смерть Enemy после fire не уничтожает projectile.

- Player Death уничтожает enemy projectiles, нацеленные на Player.

# 9. Spawn System

Normal/Elite/MiniBoss используют универсальную настраиваемую wave/cycle-модель.

Cycle  →  Wave 1 gradual spawn  →  Clear  →  Pause  →  Wave 2...  →  Final clear  →  Long real-time cooldown  →  Reset

- Потери текущей волны не восполняются по одному.

- Количество волн, врагов, spawn delay, inter-wave delay и cooldown задаются данными.

- Длинные cooldown продолжаются при закрытой игре.

- Main Boss постоянно доступен, если не находится на своём real-time respawn cooldown.

- Main Boss можно атаковать напрямую без kill-counter или summon condition.

# 10. Boss Progression

## 10.1. Main Boss

- Первый kill Main Boss навсегда открывает следующую Zone.

- Первый kill даёт обычный Boss LootTable + отдельную Guaranteed First Kill Reward.

- First Kill Reward задаётся индивидуально для Boss и выдаётся один раз.

- Повторные kills дают только обычный Boss LootTable.

- После смерти Boss запускается configurable real-time cooldown, базовый ориентир около 60 минут.

- Cooldown продолжается при закрытой игре.

- Не создаётся отдельный уникальный named resource для каждого Boss.

# 11. Death and Respawn

- Смерть мягкая: Level, EXP, Equipment, Resources, Zones и Loot не теряются.

- После короткой configurable задержки (ориентир ~2 сек) Player автоматически respawn в Safe Zone.

- Во время death delay движение, атака и взаимодействия отключены.

- После respawn Player получает configurable долю Max HP, рабочий ориентир ~10%, а не Full Heal.

- Все открытые gameplay/UI окна закрываются.

- Player-owned active projectiles уничтожаются.

- Enemy projectiles targeting Player уничтожаются.

- Combat State сбрасывается.

- Если Enemy реально умер до завершения Player Death event, награда может быть выдана; незавершённые атаки после Player Death отменяются.

# 12. Level и EXP

## 12.1. Level Cap

Максимальный Player Level — 100. На Level 100 EXP полностью перестаёт накапливаться. Overflow EXP не хранится.

## 12.2. Level Up

Каждый полученный уровень до 100 создаёт обязательный выбор 1 из 3 случайных постоянных Upgrade текущего цикла.

- Damage +%

- Attack Speed +%

- Max HP +%

- Movement Speed +%

- Critical Chance +%

- Attack Range +%

Конкретные значения балансируются позже.

- Level Up UI не ставит мир на паузу.

- Пока окно выбора открыто, Player Movement отключено, но Auto Attack продолжает работать.

- Quick Slots остаются доступны.

- Окно нельзя закрыть, отложить или пропустить.

- Несколько уровней образуют последовательную очередь выборов.

- EXP продолжает начисляться, пока окно открыто, поэтому очередь может расти.

- При смерти окно закрывается, очередь сохраняется и после respawn снова открывается через короткую задержку.

- Level Up имеет приоритет над обычным UI: Inventory/NPC UI закрывается и автоматически не возвращается.

# 13. Rebirth / Перерождение

## 13.1. Общая схема

Level 1  →  Level 100  →  Rebirth I  →  Level 1→100  →  Rebirth II  →  Level 1→100  →  Rebirth III  →  Level 1→100  →  Final Level 100

В текущей версии максимум 3 Rebirth. После Rebirth III Player ещё раз развивается до Level 100, после чего Level остаётся 100, EXP не начисляется, Level Up больше не создаётся, Rebirth IV отсутствует.

## 13.2. Что сбрасывает Rebirth

## 13.3. Rebirth Power

Каждое успешное Rebirth увеличивает Rebirth Rank и усиливает один и тот же вечный комплексный эффект Rebirth Power.

- Damage

- Max HP

- Defense

- Health Regeneration

Точные коэффициенты на Rank балансируются позже. Rebirth Power не сбрасывается.

## 13.4. Подготовка к ритуалу

Level 100 автоматически активирует соответствующий Rebirth Quest I/II/III. Quest System в текущей версии используется только для Rebirth, но архитектурно должна позволять расширение позже.

- Собрать требуемые Monster Parts и специальные ресурсы.

- Получить требуемые Boss/Dungeon-category материалы без обязательного уникального ресурса каждого Boss.

- Создать Ritual Stone текущего ранга через Crafting.

- Прийти в Ritual Zone.

- Полностью снять все 8 предметов Equipment.

- Использовать Ritual Stone и начать ритуал.

Ritual Stone I/II/III — crafted key item. Рецепт соответствующего камня открывается при Level 100 на текущем этапе Rebirth. Требования растут от Rebirth I к III.

## 13.5. Ритуал

- Ритуал длится ориентировочно 10 секунд.

- За это время Player атакуют слабые ritual monsters в 3 волнах.

- Каждый следующий Rebirth использует более требовательные материалы и более угрожающие волны.

- Во время ритуала действует скрытая Ritual Protection: Current HP не может опуститься ниже 1.

- Игроку не сообщается о защите. UI и presentation, наоборот, создают ощущение необратимого риска и предупреждают, что смерть прервёт ритуал и второй попытки не будет.

- После третьей волны ритуал успешно завершается и выполняется Rebirth.

# 14. Equipment

## 14.1. Слоты

- Нет OffHand, второго Ring и Cape.

- Нет Player Level requirements для Equipment.

- После Rebirth Level 1 может использовать любую сохранённую экипировку.

- Нет Durability, wear, breakage или Repair.

- Нет Transmog/skin override system.

- Gameplay Equipment отделён от EquipmentVisual/presentation.

## 14.2. Rarity

Рабочая шкала: Common, Uncommon, Rare, Epic, Legendary.

- Rarity отражает ценность и сложность получения, но не является универсальным stat multiplier.

- Статы предметов фиксированы и заранее определены.

- Цвет rarity используется только в UI: рамки, названия, loot notifications, Merchant, Crafting и т. п.

- 3D-модель не меняет цвет/свечение только из-за rarity.

## 14.3. Max HP при смене статов

- Изменение Max HP сохраняет абсолютный Current HP.

- Пример: 80/100 → +100 Max HP = 80/200.

- Если Max HP уменьшается ниже Current HP, Current HP clamp до нового Max HP.

- Само изменение статов не может убить Player.

# 15. Equipment Level

- Все 8 Equipment slots используют Item Level от 1 до 10.

- Новый предмет создаётся Level 1 и Efficiency 100%.

- Upgrade: Main Item LvN + 1 идентичный exact ItemDefinition donor + Gold + Fragments нужной rarity → LvN+1.

- Upgrade гарантирован, без failure/catalyst/protection.

- Donor используется один независимо от его Level/Efficiency; его инвестиции не дают дополнительного кредита.

- Locked item нельзя использовать как donor.

- Donor с Gems требует предупреждения; при подтверждении Gems уничтожаются, если заранее не Extract/Reforge.

- Глобальная таблица LevelMultiplier ориентировочно растёт от 100% на Lv1 до 190% на Lv10; точные значения балансируются.

Intrinsic item stats масштабируются как Item Stat × LevelMultiplier × Efficiency. Gem stats масштабируются Efficiency, но не Equipment Level.

# 16. Efficiency

- Каждый item instance имеет собственную Efficiency.

- Старт 100%, текущий максимум 120%.

- Efficiency усиливает intrinsic item stats и socketed Gem bonuses.

- Повышается специальным материалом; шаги и шансы data-driven.

- Fail не понижает Efficiency: ресурсы расходуются, значение остаётся.

- Стоимость растёт; высокие ступени могут требовать редкие Boss/Dungeon-category материалы.

- Efficiency Catalyst повышает chance, но не доводит его до 100%; tiers допускаются.

- При Reforge Efficiency не переносится.

# 17. Gems

## 17.1. Основы

- Каждый Equipment имеет 3 sockets.

- Gem families: Ruby/Attack, Sapphire/Defense, Emerald/Max HP, Topaz/Attack Speed, Amethyst/Crit Chance, Aquamarine/Health Regen.

- Названия/визуалы могут измениться позже.

- Каждый Gem data-driven определяет допустимые Equipment slots.

- Gem Level: 1–9.

- В мире, у врагов и в Dungeon выпадает только Lv1.

- Более сильный контент повышает шанс/качество варианта, но не уровень Gem.

- Одинаковый exact Gem type нельзя вставить дважды в один item, но разные variants одного stat допустимы.

## 17.2. Gem Upgrade

Строгое правило: Main Gem LvN + такой же exact Gem Lv1 donor → попытка получить LvN+1.

- Lv2 + Lv2 не используется.

- Lv1→Lv2 гарантирован 100%.

- Поздние уровни имеют configurable success chance.

- Fail: Main Gem теряет 1 level, donor и Gold расходуются.

- Protection Stone предотвращает downgrade при fail, но сам расходуется.

- Catalyst повышает success chance; tiers допустимы; cap ниже 100%.

- Protection + один Catalyst можно использовать одновременно.

- Gem можно улучшать прямо установленным в Equipment.

## 17.3. Installation

- Одна Gem за операцию.

- Выбор Equipment → пустой compatible socket → Gem → Gold.

- Installation гарантирована.

- Occupied socket нельзя перезаписать; сначала Extraction.

## 17.4. Extraction

- Ручного безопасного снятия Gem нет.

- Blacksmith выполняет платную risky Extraction.

- Можно выбрать конкретные sockets или Extract All.

- Каждый выбранный Gem roll независимо: Success / Downgrade / Destroy.

- При Success Gem того же level возвращается в Inventory.

- При Downgrade возвращается на один level ниже; Lv1 downgrade означает потерю.

- После операции socket всегда становится пустым.

- Extraction Catalyst один на всю операцию и модифицирует все rolls.

- Самый сильный Catalyst может убрать Destroy chance, но не делает extraction полностью безопасной.

## 17.5. Gem Recycling / Jeweler

- Gem можно dismantle в Gem Shards.

- Yield зависит от Gem Level и intrinsic value варианта; процесс убыточный.

- Jeweler: Shards → выбрать stat family → получить случайный Lv1 variant этой family.

- Игрок контролирует family, но не exact variant.

# 18. Reforge

- Переносит весь Gem setup со старого предмета на новый compatible item.

- Destination sockets должны быть пустыми.

- Gems сохраняют type/level и не рискуют.

- Equipment Level и Efficiency не переносятся.

- Weapon Reforge — только между одинаковым subtype: Sword→Sword, Bow→Bow и т. д.

- Для остальных предметов требуется тот же Equipment slot.

- Source destruction chance configurable, рабочий ориентир 30%.

- Protection для Reforge нет.

- Даже при уничтожении Source Gems остаются безопасно перенесёнными.

- Стоимость — Gold; отдельного ReforgeStone нет.

# 19. Inventory и Item Lock

- Категории: Equipment, Materials, Gems, Consumables, Blueprints.

- Currencies: Gold и Piastres.

- Inventory не имеет общего slot/capacity limit.

- Stackable items агрегируются.

- Consumables не имеют stack limit.

- Unique Equipment хранит instance state: Efficiency, Level, Gems, Lock и т. д.

- Equipment instance может быть Locked.

- Locked item нельзя продать, dismantle или использовать как donor.

- Unlock выполняется вручную.

# 20. Loot System

## 20.1. Доставка Loot

Физических pickup-объектов для основной добычи нет. Enemy Death сразу записывает награду в соответствующее хранилище.

- Gold → Currency

- Materials → Inventory

- Equipment → Inventory

- Gems → Inventory

- Blueprints → Inventory

Обычный loot показывает компактные уведомления; редкий Equipment — более заметные; Legendary — усиленная presentation позже.

## 20.2. Источники

## 20.3. LootTable

- Каждый конкретный Enemy definition/type имеет собственный configurable LootTable.

- Группы: Currency, Materials, Gems, Equipment.

- Используются DropChance, Rolls и weighted Entries.

- Искусственный Nothing entry не требуется.

# 21. Resources и Economy

Игра использует несколько специализированных ресурсов вместо одного универсального материала.

- Примеры: Iron, Crystal, EfficiencyStone, MonsterCore, BossCore, SacredEssence, LegendaryShard.

- Новые зоны могут вводить новые типы ресурсов, но старые должны сохранять полезность.

- Gold — основная сервисная валюта и sink: Equipment Level, Efficiency, Gem operations, Reforge, Healer/Blessing и т. д.

- Piastres — отдельная premium/специальная валюта; конкретная монетизация определяется позже.

- Fragment exchange вверх бесплатный.

# 22. Dismantle, Sell и Fragments

## 22.1. Equipment

- Equipment можно продать за Gold или dismantle в Equipment Fragments.

- Fragments универсальны внутри rarity.

- Yield зависит от Rarity + Equipment Level, но не от Efficiency.

- Gold/Efficiency investments не возвращаются.

- Equipment с Gems можно продать/dismantle только после сильного предупреждения: Gems будут уничтожены.

## 22.2. Fragment Exchange

Рабочая базовая цепочка: 100 Common → 1 Uncommon → 1 Rare → 1 Epic → 1 Legendary. Коэффициенты configurable.

- Обмен только вверх.

- Reverse exchange отсутствует.

- Допускается прямой multi-tier auto-chain.

- Обмен бесплатный.

## 22.3. Blueprints

- Duplicate Blueprint можно dismantle в Blueprint Components.

- Blueprint NPC позволяет выбрать rarity tier и заплатить Components за случайный Blueprint этой rarity.

- Duplicate protection исключает уже изученные Blueprints, пока существуют неизученные eligible.

- Если все eligible изучены, покупка блокируется без расхода ресурсов.

# 23. Crafting и Blueprints

- Базовые recipes доступны со старта.

- Часть recipes открывается progression/world milestones.

- Высокоценные/Epic/Legendary recipes открываются через Blueprint из Boss/Dungeon content.

- Blueprint физически находится в Inventory до изучения.

- Использование Blueprint навсегда открывает recipe, после чего Blueprint исчезает.

- Crafting мгновенный: без очередей и offline craft timers.

- Stackable Consumables поддерживают Craft ×1, ×10, Max.

# 24. Merchant / Buyer

## 24.1. Permanent Stock

- Ограниченный stock.

- Постепенный real-time restock.

- Per-offer настройки: Item, Price, Currency, MaxStock, RestockAmount, Interval.

- Restock продолжается при закрытой игре и ограничивается MaxStock.

## 24.2. Rotating Stock

- Случайный ассортимент с ограниченным stock.

- Обновляется по configurable real-time timer, продолжающемуся offline.

- На refresh ассортимент заменяется.

## 24.3. Доступность

- Equipment: Common — да, Uncommon — да, Rare — иногда rotating, Epic/Legendary — нет.

- Resources: Common — permanent, Uncommon — permanent/rotating, Rare — rotating, Epic/Legendary/Boss-specific — нет.

- Buyer покупает Equipment, Materials и Consumables за Gold.

- Gems перерабатываются через Shards, Blueprints — через Components.

# 25. Healer и Blessing

- Healer мгновенно полностью восстанавливает HP.

- Также продаёт комплексный временный Blessing package.

- Standard ~+10% за Gold.

- Improved ~+20% за более сложный gameplay resource.

- VIP ~+30% за Piastres.

- Blessing может включать Movement Speed, Attack Speed, Defense, Health Regen и другие согласованные stats.

- Blessings не stack: более сильный/current заменяет слабый.

- Duration configurable, рабочий ориентир 10 минут.

- Timer real-time и продолжается при закрытой игре.

- Blessing заканчивается по expiry или Player Death.

# 26. Consumables и Quick Slots

## 26.1. Quick Slots

- Начальная концепция — 3 Quick Slots.

- PC: клавиши 1/2/3; mobile — HUD buttons позже.

- Slot привязан к ConsumableDefinition/type.

- HUD показывает icon, quantity и cooldown.

- При quantity 0 assignment остаётся и автоматически оживает при получении новых предметов.

- Один Consumable type нельзя назначить в несколько slots; новое назначение переносит его.

- Quick Slots работают во время Level Up UI.

## 26.2. Health Potions

- Несколько tiers.

- Лечат процент от текущего Max HP.

- Все Health Potions используют общий configurable Health Potion Cooldown.

- Попытка использовать при Full HP отклоняется: item не расходуется, cooldown не стартует.

- Overheal отсутствует.

- Cooldown real-time и продолжается при закрытой игре.

## 26.3. Combat Buff Potions

- Временные специализированные buffs: Damage, Defense, Move Speed, Accuracy и т. д.

- Разные effect categories могут быть активны одновременно.

- Одинаковая category не stack.

- Повтор той же силы расходует potion и полностью обновляет duration.

- Более сильный buff заменяет слабый.

- Попытка применить более слабый при активном сильном отклоняется без расхода.

- Player Death снимает все active combat potion buffs.

- Duration real-time, включая закрытую игру.

- Equipment swap не снимает buff.

- Blessing и Combat Potion — разные sources и могут одновременно влиять на один stat.

- Combat Buff Potions используют общий короткий cooldown, независимый от Health Potion cooldown.

## 26.4. Получение

- Basic potions — Merchant.

- Enemy LootTable.

- Boss/Dungeon — более сильные/редкие.

- Crafting.

- Сильнейшие варианты не находятся постоянно у обычного Merchant.

- Recipes используют общую Blueprint/Crafting систему.

# 27. Offline / Idle Progression

## 27.1. Offline Reward Profile

Offline combat не симулируется. Каждая боевая зона имеет data-driven OfflineRewardProfile.

- EXP per hour.

- Gold per hour.

- Обычные eligible Resources per hour.

## 27.2. Какая зона используется

Offline Reward Zone — зона, в которой Player последним реально убил qualifying ordinary enemy. Простое посещение более сильной зоны не меняет entitlement.

- Обычная Player Death не сбрасывает Offline Reward Zone.

- Rebirth сбрасывает Offline Reward Zone.

- После Rebirth Player должен заново убить обычного врага в нужной зоне.

- Если Level 1 после Rebirth способен убить врага Zone 5, он получает право на AFK-награды Zone 5.

## 27.3. Время

- Минимальный порог отсутствия — configurable, рабочий ориентир 10 минут.

- Максимальное накопление — 24 часа.

- Меньше порога — награды нет.

- От порога до 24 часов — фактическое eligible время.

- Больше 24 часов — только 24 часа.

## 27.4. Что можно получить

## 27.5. Возврат в игру

- После загрузки показывается Offline Rewards summary: время отсутствия, EXP, Gold, Resources.

- Offline EXP может дать несколько Levels.

- Каждый реально полученный Level создаёт обязательный 1-of-3 Level Up Choice.

- После summary игрок последовательно разбирает очередь Level Up.

- Offline EXP может довести до Level 100.

- EXP сверх Level 100 сгорает.

- Достижение Level 100 offline активирует соответствующий Rebirth Quest.

# 28. Save System

## 28.1. Local + Yandex Cloud

- Gameplay state автоматически сохраняется локально.

- При доступности синхронизируется с Yandex Cloud Save.

- Сохранение запускается при важных progression/economy events.

- Дополнительно configurable periodic autosave, рабочий ориентир 30–60 секунд.

- Предпринимается save при подходящих lifecycle events: потеря фокуса/корректное закрытие.

## 28.2. Конфликт Local / Cloud

Выбирается самое свежее валидное сохранение по timestamp. Нельзя определять актуальность по Player Level, Gold или Rebirth Rank.

## 28.3. Надёжность

- Current Save + Backup Save.

- После успешного save предыдущий валидный Current сохраняется как Backup.

- При load выполняется validation; при повреждении используется fallback Backup/Cloud.

- Save содержит SaveVersion.

- Будущие обновления используют migrations старых save formats.

# 29. Новая игра

GreatSword и Bow не имеют отдельной unlock/mastery системы. Это обычное Equipment: как только предмет получен через loot/crafting/Merchant, его можно сразу экипировать.

# 30. UI и пауза

- Обычный gameplay UI не ставит игру на паузу.

- Inventory, Blacksmith, Merchant, Crafting, Healer, Jeweler и другие сервисные окна работают в real-time.

- Level Up также не ставит мир на паузу, но блокирует Player Movement.

- Player Death закрывает все открытые gameplay/UI окна.

- Rarity color coding используется только в UI.

- HUD должен поддерживать HP, EXP/Level, Quick Slots, важные cooldowns, Mini Map и необходимые combat/progression indicators.

# 31. Системы, которые намеренно НЕ входят в текущий дизайн

- Infinite WorldLevel / автоматический бесконечный scaling мира.

- Отдельные roguelite runs.

- PvP и multiplayer.

- Player active combat skills с отдельными кнопками.

- Mana/Energy/Rage без будущей конкретной необходимости.

- Equipment durability/repair.

- Transmog/cosmetic override system.

- Fast Travel / Waypoints / Return Stone.

- Level requirements для Equipment.

- Обычные Main/Side Quests — Quest System пока используется только для Rebirth.

- Уникальный отдельный material для каждого Boss.

- Физические loot pickups для основной добычи.

- Offline получение Equipment/Gems/Blueprints/Legendary/Boss-Dungeon rare loot.

- Монетизация как ранний приоритет разработки.

# 32. Основные data-driven сущности

Финальные имена классов могут уточняться, но архитектурно проекту потребуются отдельные определения данных для следующих сущностей:

- EnemyDefinition / EnemyType / Enemy LootTable

- WeaponDefinition / WeaponType

- EquipmentDefinition / Equipment instance data

- GemDefinition / Gem family / Gem level rules

- ConsumableDefinition / BuffDefinition

- LootDefinition / LootTable

- ZoneDefinition / OfflineRewardProfile

- SpawnPoint/SpawnCycle configuration

- Boss configuration / First Kill Reward / Respawn cooldown

- RecipeDefinition / BlueprintDefinition

- Merchant Offer / Permanent & Rotating Stock

- RebirthDefinition I–III / Ritual requirements / Ritual Stone recipe

- QuestDefinition для Rebirth

- Player progression / Rebirth Rank / map discovery / save data

# 33. Приоритет разработки

Главный принцип остаётся: маленький работающий этап → тест → следующий этап. Не реализовывать одновременно десятки систем.

- Стабилизировать существующий Player/Enemy/Combat/Spawn/Level Up прототип под зафиксированные правила.

- Привести Death/Respawn и Level Up UI к финальной real-time логике.

- Добавить базовые combat stats: Defense, Accuracy, Evasion и общий damage pipeline.

- Завершить Sword/GreatSword/Bow behaviors.

- Реализовать Zone progression и Main Boss first-kill unlock без WorldLevel.

- Реализовать Inventory + Equipment instances + 8 slots.

- Реализовать Loot/Resources/Currencies без физических pickups.

- Реализовать Equipment Level, Fragments и Item Lock.

- Реализовать Gems, Blacksmith operations, Efficiency и Reforge.

- Реализовать Crafting/Blueprints/Merchant/Jeweler/Healer/Consumables.

- Реализовать Dungeon content model.

- Реализовать Level 100 + Rebirth I–III + Ritual Zone + Rebirth Quest.

- Реализовать Offline Rewards.

- Реализовать SaveSystem Local + Backup + Yandex Cloud.

- Добавить Mini Map, World Map и Fog of War.

- После полностью работающего gameplay заменить primitives реальными моделями и заняться presentation/UI polish.

# 34. Параметры, оставленные на балансировку

Следующие значения сознательно не фиксируются как окончательные и должны храниться в данных/настройках:

- EXP curve Level 1–100.

- Точные проценты Level Up upgrades.

- Rebirth Power coefficients для Rank I–III.

- Enemy stats каждой зоны.

- Accuracy/Evasion formula и stat caps.

- Defense formula.

- GreatSword AoE geometry/MaxTargets.

- Boss/Spawner cooldowns для конкретного контента.

- Loot chances, Rolls, weights и item rarity distribution.

- Equipment Level multipliers и costs.

- Efficiency step/chances/costs.

- Gem upgrade/extraction probabilities и catalysts.

- Merchant prices/restock intervals.

- Potion heal %, buff strengths/durations/cooldowns.

- Blessing values/costs.

- OfflineRewardProfile rates.

- Rebirth ritual material quantities и конкретные wave compositions.

# 35. Итоговая игровая петля

Safe Zone  →  Zone farming  →  EXP + Loot + Resources  →  Level Up choices  →  Equipment/Gems/Crafting  →  Elite/MiniBoss  →  Main Boss  →  Next Zone  →  Dungeon farming  →  Level 100  →  Rebirth Quest  →  Ritual  →  Rebirth  →  Повторное развитие

После Rebirth III и финального достижения Level 100 мета-прогрессия текущей версии заканчивается, но Equipment/Gems/Efficiency/Crafting/Boss/Dungeon/Resource farming продолжаются. Будущие обновления могут добавить новые зоны и следующую ступень мета-прогрессии без необходимости возвращать старую систему бесконечного WorldLevel.

# 36. Unity MCP / Codex automation rules

Этот раздел определяет, как Codex должен работать с открытым Unity-проектом через MCP for Unity (CoplayDev/unity-mcp). GDD остаётся источником истины по gameplay-дизайну; MCP используется для инспекции, создания и настройки Unity-объектов, компонентов, сцен, ScriptableObject, Prefab, Inspector-ссылок и проверки результата.

## 36.1. MCP-first workflow

- Перед сложными изменениями Codex должен проверить состояние Unity Editor и убедиться, что Editor готов принимать команды.

- Перед созданием новых объектов Codex сначала инспектирует существующую Scene hierarchy, Prefabs, Components, ScriptableObjects и Project packages, чтобы не создавать дубликаты.

- Если в проекте запущено несколько Unity Editor, Codex обязан выбрать правильный instance проекта до внесения изменений.

- После создания или изменения C#-скриптов Codex ждёт завершения компиляции и проверяет Unity Console на Errors/Warnings.

- Если после изменения кода требуется объект, компонент, ссылка в Inspector, Prefab, ScriptableObject, Tag/Layer, Input Action или Scene-настройка, Codex по возможности выполняет это сам через Unity MCP, а не перекладывает ручную настройку на пользователя.

- После изменений Scene/Prefab Codex сохраняет результат через Unity Editor/MCP.

- Для визуально значимых изменений Codex делает screenshot Scene View/Game View через MCP и проверяет результат.

- Для gameplay-систем, которые можно проверить автоматически, Codex использует Unity Test Framework через MCP. Ручной тест-сценарий всё равно указывается пользователю.

## 36.2. Предпочтительные MCP operations

## 36.3. Правило ручной настройки

Ручные шаги в Unity допустимы только если конкретная операция недоступна через подключённый MCP, требует решения пользователя, является потенциально разрушительной или MCP вернул ошибку, которую нельзя безопасно устранить автоматически. В этом случае Codex должен явно указать: что именно не удалось автоматизировать, почему и какие минимальные ручные шаги остаются.

## 36.4. Безопасность изменений

- Не удалять Scene objects, Prefabs, ScriptableObjects или Components только потому, что они кажутся неиспользуемыми; сначала проверить зависимости.

- Перед массовыми изменениями Scene/Prefab предпочтительно использовать Undo-friendly Unity Editor operations и выполнять изменения небольшими пакетами.

- Не менять Tags/Layers/Input Actions/Project Settings глобально без проверки существующих значений.

- Не создавать второй объект/компонент, если подходящий уже существует и может быть переиспользован.

- Не использовать execute_code как универсальную замену специализированным MCP tools. Сначала использовать специализированный tool; execute_code — только когда он объективно упрощает корректную Editor-операцию.

- После автоматических изменений обязательно проверить Console и фактическое состояние Scene/Prefab/Inspector.

## 36.5. Definition of Done для каждого этапа

- C# код создан/изменён.

- Unity завершил компиляцию без новых compile errors.

- Необходимые GameObjects/Components/ScriptableObjects/Prefabs созданы или изменены через MCP.

- Inspector references назначены.

- Scene/Prefab сохранены.

- Console проверена.

- По возможности выполнен автоматический EditMode/PlayMode test.

- Для визуальных изменений проверен screenshot.

- Codex перечислил изменённые assets и дал пользователю короткий ручной test scenario.

## 36.6. Codex project companion files

Чтобы Codex стабильно соблюдал правила проекта и не перечитывал весь GDD как инструкцию по операционной работе, рядом с Unity-проектом рекомендуется хранить текстовые companion-файлы:

- AGENTS.md — обязательные инструкции Codex для этого репозитория, включая MCP-first workflow.

- Docs/Lootbound_Isles_GDD.md — текстовая Markdown-копия GDD, удобная для поиска и ссылок из Codex.

- Docs/PROJECT_STATE.md — краткое текущее состояние реализованных систем; обновляется после завершённых этапов.

- Docs/UNITY_MCP_WORKFLOW.md — детальные правила работы через MCP и checklist проверки.