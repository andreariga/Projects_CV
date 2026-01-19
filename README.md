# Software Development Portfolio

This repository hosts two distinct capstone projects demonstrating advanced proficiency in the .NET ecosystem, ranging from cross-platform mobile development with AI integration to high-performance full-stack web architectures.

## Projects Overview

1.  **Alice Neural:** An AI-powered voice assistant built with .NET MAUI and Azure Cognitive Services.
2.  **PokéManager:** A Full Stack Web Application with a .NET Minimal API backend and a responsive frontend.

---

# 1. Alice Neural (Mobile Voice Assistant)

**Alice Neural** is a cross-platform mobile application acting as an intelligent voice interface. It bridges user voice commands with various web services through a custom-trained Conversational Language Understanding (CLU) model.

### Architecture & Key Features

#### Artificial Intelligence & NLP
* **Azure CLU Integration:** Configured and trained a custom Conversational Language Understanding model to classify intents (e.g., Weather, Navigation, General Knowledge) and extract semantic entities from speech.
* **Speech Services:** Implemented Azure Speech SDK for continuous Speech-to-Text and high-fidelity Text-to-Speech synthesis.

#### Backend Logic & Data Handling
* **API Orchestration:** The application manages asynchronous calls to multiple REST endpoints:
    * **Open-Meteo:** Complex weather forecasting based on real-time geolocation.
    * **Bing Maps:** Geocoding services for navigation and location-based queries.
    * **Wikipedia API:** Direct JSON data consumption for general knowledge retrieval.
* **Custom Data Parser:** Developed a specialized C# algorithm to deserialize and sanitize incoming JSON data (specifically from Wikipedia), stripping formatting artifacts to ensure the vocal output is clean and natural.

#### Tech Stack
* **Framework:** .NET MAUI (C# 10)
* **Cloud:** Microsoft Azure (Cognitive Services, CLU)
* **Patterns:** Asynchronous Programming (Async/Await), Dependency Injection.

---

# 2. PokéManager (Full Stack Web App)

**PokéManager** is a comprehensive web solution for managing Trading Card Game collections. It features a robust backend API built with the latest .NET standards and a responsive Single Page Application (SPA) frontend.

### Architecture & Key Features

#### Backend (.NET Web API)
* **Minimal APIs:** Utilized .NET's modern Minimal API architecture (`MapGroup`) for lightweight, high-performance endpoint definition, reducing boilerplate code.
* **Security & Stability:** Implemented **Rate Limiting** (Fixed Window Policy) middleware to protect endpoints from abuse and manage traffic load efficiently.
* **Data Layer:** Designed a relational data model using **Entity Framework Core** connected to a **MySQL** database for storage of user collections and decks.
* **Proxy Service Pattern:** Created a backend proxy layer to fetch and aggregate data from third-party external APIs (PokeAPI, TCGPlayer), decoupling the frontend from direct external dependencies.
* **Documentation:** Fully integrated OpenAPI (Swagger) for automated endpoint documentation and testing.

#### Frontend
* **Responsive Design:** Developed a custom UI using HTML5 and CSS3 that adapts to various device form factors.
* **Asynchronous Interaction:** Implemented vanilla JavaScript with AJAX to consume backend APIs dynamically, updating the DOM without page reloads.

#### Tech Stack
* **Backend:** C#, .NET 8/9, Entity Framework Core, MySQL.
* **Frontend:** HTML5, CSS3, JavaScript.
* **Tools:** Swagger UI, Postman.

---


*Project developed by Rigamonti Andrea*
