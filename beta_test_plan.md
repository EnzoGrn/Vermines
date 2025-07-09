# **BETA TEST PLAN – OMGG**

## **1. Core Functionalities for Beta Version**
This beta testing aims to validate the essential features of the Vermines game developed as part of OMGG. The tests will ensure stability, playability and the overall user experience.

| **Feature Name**  | **Description** | **Priority (High/Medium/Low)** | **Changes Since Tech3** |
|-------------------|---------------|--------------------------------|--------------------------|
| Gameplay system | Manages rules, turns, actions and player interactions. | High | - |
| Multiplayer & Network | Multiplayer game, sending information, etc. | High | Network made with Photon Fusion now. |
| User interface | Displays essential information (cards, scores, history of actions) | High | Refined design and simplified navigation. |
| Customised rules | Allows players to configure specific rules to personalise games. | Low | - |
| Animations and visual feedback | Improves immersion through fluid and intuitive animations. | Medium | Integration of DOTween and Splines for more natural transitions. |

---

## **2. Beta Testing Scenarios**

### **2.0 How the game works**

**Vermines** is a deck-building card game for 2 to 4 players.

In a deck-building game, each player begins with a basic deck of cards that grows and evolves throughout the game as they adopt different strategies.

In Vermines, you take on the role of an insect clan—beetles, ladybugs, ants, and more—seeking to gain favor with a god. Your ultimate goal is to summon this deity and claim victory.

To achieve victory, you’ll need to manage several key elements:

- **Eloquence:** This is the game’s main currency. It allows you to recruit partisan—the Vermines characters—and to acquire utility cards that improve your deck and overall strategy.

- **Souls:** These are victory points. You earn them by sacrificing your partisans to your god. The more souls you gather, the closer you are to summoning your deity and winning the game.

You’ll also need to perform various actions during your turn:

- **Play a partisan:** This places the card in front of you on the table. Later in the game, you’ll be able to activate the card’s ability, called a card effect.

- **Discard a card:** This sends a card to a different pile in your deck, allowing you to temporarily set aside cards you don't want to use right away. You'll get them back later in the game.

- **Sacrifice a partisan:** Doing this grants you souls for your god. It's a strategic move to gain victory points at the cost of losing a partisan.

Each player's turn consists of four distinct phases:​

- **Sacrifice Phase:** You may sacrifice up to two cards from your hand to gain souls point, wich represents the victory points.

- **Gain Phase:** Use cards effect from your table to perform actions, such as acquiring new cards, gain eloquences, or triggering special abilities.​

- **Game Phase:** Use eloquences accumulated during the Action Phase to buy new cards from the market or courtyard, adding them to your discard pile for future use.​

- **Resolution Phase:** Discard any remaining cards in your hand and draw a new hand of five cards from your deck. If your deck is empty, shuffle your discard pile to form a new deck. Shops are refilled at this moment and the game continues. The game ends when one player reaches 100 souls points.

### **2.1 User Roles**
The tests will be carried out according to the different player roles.

| **Role Name** | **Description** |
|---------------|---------------|
| Game Host | Creates sessions, manages the game rules and has authority over the game. |
| Player | Plays the game and can interact with the game mechanics, without any special privileges. |

---

### **2.2 Test Scenarios**

#### **Scenario 1: Gameplay system**
- **Role Involved:** Game Host / Player
- **Objective:** Validate that the rules are correctly applied.
- **Preconditions:**
  1. Have a working client of the game.
  2. Have access to the ‘Quick Play’ functionality.
  3. Have at least one second player to play with.
- **Test Steps:**
  1. Run the game client.
  2. Click on Quick Game.
  3. Wait for the game to start, once the second player has joined.
  4. Check that our eloquence value has risen to 2.
  5. Check that we have 2 cards in our hand.
  6. Discard all our cards.
  7. Proceed to the next turn.
  8. Check that you have 3 cards in your hand.
  9. Wait for our next turn.
  10. Buy a card in the shop.
  11. Play a partisan card.
  12. Discard the rest of the cards.
  13. End your turn.
  14. Sacrifice the partisan you played in the previous round.
- **Expected Outcome:** The gameplay respects the rules without bugs or inconsistencies.

#### **Scenario 2: User Interface**
- **Role Involved:** Player
- **Objective:** To test the ergonomics and readability of the information displayed.
- **Preconditions:**
  1. The same as Scenario 1.
  2. Have a game in progress in order to have several actions already completed.
- **Test Steps:**
  1. Check the current phase of the game.
  2. Check if the first player in the list is the player whose turn it is.
  3. Check number of eloquence/souls and the family in the player banner.
  4. Open the desk.
  5. Open the book and show the information for one player, demonstrating that it is identical to the information in the banners.
  6. Test the navigation between the different menus: global to market to courtyard to market to global.
- **Expected Outcome:** The interface is intuitive and displays all key information clearly.

#### **Scenario 3: Customised rules**
- **Role Involved:** Game Host / Player
- **Objective:** Validate that custom rules are applied and functional.
- **Preconditions:**
  1. Have a working client of the game.
  2. Have access to the ‘Party Menu’ functionality.
  3. Have at least one second player to play with.
- **Test Steps:**
  1. Launch the game client.
  2. Go to ‘Party Menu’.
  3. The **Host** must click on the ‘Create’ button.
  4. Send the game code to the other player.
  5. The **Player** must click on the ‘Join’ button.
  6. The **Host** must change the rules: **Number of eloquence at start of turn** (e.g. from 2 to 3).
  7. The **Host** must change the rule: **Number of cards drawn per turn** (e.g. from 3 to 4).
  8. Start the game.
  9. Check that you start the game with 3 eloquence.
  10. Pass your turn.
  11. Check that you have drawn 4 cards.
- **Expected Outcome:** The personalised rules are taken into account from the start of the game and do not generate any errors or inconsistencies in the game.

---

## **3. Success Criteria**

| **Criterion** | **Description** | **Threshold for Success** |
|--------------|---------------|------------------------|
| Stability | No crashes during testing. | 100% of tests without crashes. |
| Ergonomics | The interface is intuitive and interactions are fluid. | 80% positive feedback from testers |
| Key functionalities | The game mechanics work without error. | 50% of tests validated |

---

## **4. Known Issues & Limitations**

| **Issue** | **Description** | **Impact** | **Planned Fix? (Yes/No)** |
|----------|---------------|----------|----------------|
| Host migration | If the host leaves the game, it cannot be recovered by another player. | High | Yes |
| User management | Users are not stored in the database | High | Yes |
| Matchmaking | No assignment partly based on rank | Medium | Yes |

---

## **5. Conclusion**
This **Beta Test Plan** ensures rigorous validation of the game developed as part of OMGG. The tests cover the basic mechanics, the user interface and the stability of the system. The aim is to guarantee a fluid and intuitive experience before the game goes into final production. The insights from this beta phase will help us address key issues and prepare for the final version of the project.