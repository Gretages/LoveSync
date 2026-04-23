# LoveSync – Couples' Decision Support & Planning App

**LoveSync** is a cross-platform mobile application built with **.NET MAUI**, designed to make joint decisions easier and more playful for couples.
Whether it's "What should we watch?", "Where should we eat?", or "What should we do this weekend?", LoveSync helps you find the perfect match.

## Core Features

### Account & Pairing
* **Secure Auth:** Email and password-based registration and login.
* **Couples' Link:** Connect your accounts using a unique pairing code to share a private workspace.

### "Tinder-style" Voting
* **Swipe Interface:** Browse through movies, restaurants, and date ideas.
* **Match System:** Swipe right for "Like" and left for "Dislike".
* The app highlights items that both partners liked.
* **Match Lists:** Dedicated sections for your common favorite movies, food, and places.

### Statistics & Tracking
* **Detailed Stats:** Track how many items you've voted on and how many matches you've found.
* **Top Categories:** See which category has the most common ideas.

### Joint Planning Tools
* **Shared Calendar:** Add joint programs with dates, descriptions, and locations.
* **Bucket List:** A collaborative to-do list for future goals (e.g., "Summer picnic", "Hiking trip").
* **Mini Message Board:** Leave short notes and reminders for each other on a simple, shared wall.

### System Integration
* **Notifications:** Reminders for upcoming joint programs.
* **Map Integration:** Open your phone's map app directly from date and restaurant listings.

## Technical Overview

* **Framework:** .NET MAUI with C#
* **Architecture:** MVVM (Model-View-ViewModel) pattern
* **Backend:** Firebase Authentication & Realtime Database (for real-time synchronization)
* **External APIs:** Movie data integration via public database API.

## Security & Local Setup

To protect sensitive data, the `Constants.cs` file (containing Firebase API keys and URLs) is excluded from the repository.

### To run this project:
1.  Navigate to `LoveSync/Services/`.
2.  Locate `Constants.cs.example`.
3.  Rename a copy to `Constants.cs`.
4.  Enter your own **Firebase credentials** and **API keys**.
---------------------------------------------------------
*Created by [Gretages](https://github.com/Gretages)*
