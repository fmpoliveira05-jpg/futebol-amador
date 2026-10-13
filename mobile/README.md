# Futebol Amador — Android App

Kotlin/Jetpack Compose Android app for Futebol Amador. See the [main README](../README.md) for the project overview.

Originally developed by **Artur Pinto** and **Willkie Filho** for Mobile and Ubiquitous Computing, using the same API as the Software Development Laboratory project. Later contributions are described in the main README.

## Features

- **Teams and membership:** create/edit teams, take badge photos with CameraX, manage players/administrators, and send/respond to membership requests.
- **Friendlies:** filtered opponent search, invitations, date/ground negotiation, postponement, and cancellation.
- **Leagues:** standings with played, wins, draws, losses, goals for/against, goal difference, points, and form icons. A Firestore copy supports offline viewing. Browse leagues, register teams, and view fixtures. Competitive matches now come from leagues; matchmaking was removed.
- **Transfers:** filter by team/free-agent status, league, nationality, and position. Review incoming/outgoing team offers and offers awaiting player approval; administrators list players from the member screen.
- **Starting lineup:** formation, position-filtered starters, and bench on a drawn pitch. Match reports show statistics, events, and both lineups.
- **Player profile:** preferred foot, weight, status, nationality, club, totals, season history, and transfers. Players can edit their own sporting details.
- **Principal administrator:** only the team creator can demote administrators; the creator cannot be demoted or removed.
- **Match start/finish:** both administrators confirm kickoff in a lobby. Each submits their team's score/events (fouls, goals/assists, cards, substitutions); a match finishes only when scores agree.
- **Monthly calendar:** month navigation, national holidays, and coloured indicators: grey holiday, blue friendly, purple league, green finished, red cancelled, yellow postponed. Day details show reasons and actions. Cancelling a league match requires a new date.
- **Chat:** Firestore chat between administrators with a scheduled match.
- **Notifications:** Firebase Cloud Messaging for match day, invitations, membership, and schedule changes.
- **Device calendar:** matches synchronised with the native calendar.
- **Addresses and grounds:** validated and displayed through OpenStreetMap/osmdroid.
- **Phone numbers:** validated with libphonenumber.

## Architecture

- MVVM with Compose screens, `StateFlow` ViewModels, and repositories/services calling Retrofit.
- Hilt dependency injection.
- Local session persistence with Room.
- SignalR for match-start and match-finish hubs.
- `competicao` contains league/transfer/lineup DTOs, API calls, screens, and Android-independent JUnit-tested logic.
- Cloudinary image storage.

## Setup

1. Create a Firebase project, add an Android app with package `com.example.amfootball`, and copy `google-services.json` into `app/`. It is excluded from Git. For compilation only, copy `app/google-services.example.json` to that name.
2. Add the following to `local.properties`, alongside `sdk.dir`. This file is also excluded from Git:

   ```properties
   API_BASE_URL=https://your-server/
   CLOUDINARY_CLOUD_NAME=your-cloud-name
   ```

   Without `API_BASE_URL`, the team's configured server is used. Badge uploads use a signature requested from `POST api/uploads/signature`; the preset and secret remain on the server. See [security documentation](../docs/SEGURANCA.md).
3. Open `mobile/` in Android Studio, sync Gradle, and run on an emulator or Android 9+ device.

## Tests

```bash
./gradlew testDebugUnitTest           # JVM unit tests
./gradlew connectedDebugAndroidTest  # Instrumented tests; emulator required
```

Instrumented tests use Hilt and MockWebServer instead of the real API. On Windows, use `gradlew.bat` instead of `./gradlew`.

## 2026 review

- **Navigation crashes:** sending, negotiating, or postponing a match navigated to nonexistent `calendar/<id>`.
- **Positions:** reversed `Position` ordering relative to the API registered forwards as goalkeepers. A unit test now protects the contract.
- **Registration:** ignored errors allowed navigation without a session.
- **Configuration:** replaced a temporary ngrok API URL and hardcoded Cloudinary settings with `local.properties`/`BuildConfig` values.
- **SignalR events:** `replay = 1` replayed a previous match's event in the next lobby.
- **Other issues:** chat displayed the oldest 50 messages; `fetchUserId` crashed without a session; `allowBackup` was enabled while local storage contained session data.
- **Instrumented tests:** fixed stale package imports, aligned test Hilt versions with the app, and corrected Jacoco's source directory to `src/main/java`.

## Production-related controls

- **Network:** OkHttp timeouts of 10 seconds for connection, 20 for reads/writes, and 30 overall; user-facing errors through `MensagensErro`; `Idempotency-Key` on every POST; HTTP caching only for public responses.
- **Duplicate submissions:** `FormsViewModel.aSubmeter` ignores repeated submissions until the current one finishes.
- **GDPR:** privacy policy available before registration, mandatory acceptance, JSON data export through Android's share chooser, and password-confirmed account deletion in settings. Approximate location is requested only when the user asks for it during registration.
- **Session:** AES-GCM with an Android Keystore key (`CifraLocal`); cryptographic logic in `CifraAesGcm` has unit tests.
- **Release:** R8 enabled. Preserve each published version's `app/build/outputs/mapping/release/mapping.txt`; it is not packaged in the APK.

```bash
./gradlew --no-daemon --max-workers=1 assembleDebug assembleRelease testDebugUnitTest
```