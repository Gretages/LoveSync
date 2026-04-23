# LoveSync – Couples' Decision Support & Planning App

[cite_start]**LoveSync** is a cross-platform mobile application built with **.NET MAUI**, designed to make joint decisions easier and more playful for couples. [cite: 1, 3, 25] [cite_start]Whether it's "What should we watch?", "Where should we eat?", or "What should we do this weekend?", LoveSync helps you find the perfect match. [cite: 4]

## Core Features

### Account & Pairing
* [cite_start]**Secure Auth:** Email and password-based registration and login. [cite: 8]
* [cite_start]**Couples' Link:** Connect your accounts using a unique pairing code to share a private workspace. [cite: 9, 10]

### "Tinder-style" Voting
* [cite_start]**Swipe Interface:** Browse through movies, restaurants, and date ideas. [cite: 11, 12]
* [cite_start]**Match System:** Swipe right for "Like" and left for "Dislike". [cite: 13, 14] [cite_start]The app highlights items that both partners liked. [cite: 15]
* [cite_start]**Match Lists:** Dedicated sections for your common favorite movies, food, and places. [cite: 16]

### Statistics & Tracking
* [cite_start]**Detailed Stats:** Track how many items you've voted on and how many matches you've found. [cite: 16, 17, 18]
* [cite_start]**Top Categories:** See which category has the most common ideas. [cite: 19]

### Joint Planning Tools
* [cite_start]**Shared Calendar:** Add joint programs with dates, descriptions, and locations. [cite: 20]
* [cite_start]**Bucket List:** A collaborative to-do list for future goals (e.g., "Summer picnic", "Hiking trip"). [cite: 21, 22]
* [cite_start]**Mini Message Board:** Leave short notes and reminders for each other on a simple, shared wall. [cite: 23]

### System Integration
* [cite_start]**Notifications:** Reminders for upcoming joint programs. [cite: 30]
* [cite_start]**Map Integration:** Open your phone's map app directly from date and restaurant listings. [cite: 31]

## Technical Overview

* [cite_start]**Framework:** .NET MAUI with C# [cite: 25]
* [cite_start]**Architecture:** MVVM (Model-View-ViewModel) pattern [cite: 26, 36]
* [cite_start]**Backend:** Firebase Authentication & Realtime Database (for real-time synchronization) [cite: 38]
* [cite_start]**External APIs:** Movie data integration via public database API. [cite: 28, 39]

## Security & Local Setup

[cite_start]To protect sensitive data, the `Constants.cs` file (containing Firebase API keys and URLs) is excluded from the repository. [cite: 40]

### To run this project:
1.  Navigate to `LoveSync/Services/`.
2.  Locate `Constants.cs.example`.
3.  Rename a copy to `Constants.cs`.
4.  Enter your own **Firebase credentials** and **API keys**.
---------------------------------------------------------
*Created by [Gretages](https://github.com/Gretages)*
