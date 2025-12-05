package com.example.amfootball.systemTests

import android.Manifest
import android.content.Context
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onFirst
import androidx.compose.ui.test.onLast
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextInput
import androidx.test.rule.GrantPermissionRule
import com.example.amfootball.MainActivity
import com.example.amfootball.R
import com.example.amfootball.data.enums.match.TypeMatch
import com.example.amfootball.data.network.instances.FireBaseInstance
import com.example.amfootball.data.network.instances.NetworkModule
import com.example.amfootball.data.network.interfaces.BaseEndpoints
import com.example.amfootball.data.network.interfaces.provider.FcmTokenProvider
import com.example.amfootball.mockWebServer.TestFirebaseModule
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.JsonReader
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.firestore.FirebaseFirestore
import dagger.hilt.InstallIn
import dagger.hilt.android.testing.HiltAndroidRule
import dagger.hilt.android.testing.HiltAndroidTest
import dagger.hilt.android.testing.UninstallModules
import dagger.hilt.components.SingletonComponent
import okhttp3.mockwebserver.Dispatcher
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import okhttp3.mockwebserver.RecordedRequest
import org.json.JSONArray
import org.json.JSONObject
import org.junit.After
import org.junit.Before
import org.junit.Rule
import org.junit.Test
import java.time.LocalDate
import javax.inject.Singleton
import dagger.Module
import dagger.Provides
@HiltAndroidTest
@UninstallModules(NetworkModule::class, FireBaseInstance::class)
class CancelMatchSystemTest {

    @Module
    @InstallIn(SingletonComponent::class)
    object LocalMockModule {

        @Provides
        @Singleton
        fun provideFirebaseAuth(): FirebaseAuth {
            return TestFirebaseModule.provideFirebaseAuth()
        }

        @Provides
        @Singleton
        fun provideFirestore(): FirebaseFirestore {
            return TestFirebaseModule.provideFirestore()
        }

        @Provides
        @Singleton
        fun provideFcmTokenProvider(): FcmTokenProvider {
            return TestFirebaseModule.provideFcmTokenProvider()
        }
    }

    @get:Rule(order = 0)
    val hiltRule = HiltAndroidRule(this)

    @get:Rule(order = 1)
    val composeRule = createAndroidComposeRule<MainActivity>()

    @get:Rule(order = 2)
    val permissionRule: GrantPermissionRule = GrantPermissionRule.grant(
        Manifest.permission.POST_NOTIFICATIONS
    )

    private lateinit var mockWebServer: MockWebServer
    private lateinit var loginBodyJson: String
    private lateinit var loginResponseJson: String
    private lateinit var calendarJson: String
    private lateinit var detailsTeamJson: String
    private lateinit var matchToCancelJson: String
    private lateinit var dynamicExpectedDate: String

    @Before
    fun setUp() {
        loginBodyJson = JsonReader.readJsonFromAsserts("LoginAdminTeam.json")
        loginResponseJson = JsonReader.readJsonFromAsserts("ProfileAdmin.json")
        calendarJson = JsonReader.readJsonFromAsserts("CalendarOfTeam.json")
        detailsTeamJson = JsonReader.readJsonFromAsserts("DetailsTeam.json")

        val originalMatchJson = JsonReader.readJsonFromAsserts("MatchToCancel.json")
        val matchJsonObject = JSONObject(originalMatchJson)
        val futureDate = LocalDate.now().plusDays(5)
        dynamicExpectedDate = futureDate.toString()
        matchJsonObject.put("gameDate", "${dynamicExpectedDate}T19:30:00")
        matchToCancelJson = matchJsonObject.toString()

        mockWebServer = MockWebServer()
        mockWebServer.start(8080)

        var isMatchCancelled = false

        mockWebServer.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse {
                val path = request.path ?: ""
                val url = request.requestUrl

                return when {
                    path.contains("${BaseEndpoints.AUTH_API}/login") && request.method == "POST" -> {
                        MockResponse().setResponseCode(200).setBody(loginResponseJson)
                    }
                    path.contains("${BaseEndpoints.TEAM_API}/opponent/") && request.method == "GET" -> {
                        MockResponse().setResponseCode(200).setBody(detailsTeamJson)
                    }
                    path.contains("${BaseEndpoints.CALENDAR_API}/") && path.contains("/CancelMatch/") && request.method == "DELETE" -> {
                        isMatchCancelled = true // Atualiza estado
                        MockResponse().setResponseCode(204)
                    }
                    path.contains("${BaseEndpoints.CALENDAR_API}/") && request.method == "GET" && path.count { it == '/' } >= 4 -> {
                        MockResponse().setResponseCode(200).setBody(matchToCancelJson)
                    }
                    path.contains("${BaseEndpoints.PLAYER_API}/device-token") && request.method == "PUT" -> {
                        MockResponse().setResponseCode(204)
                    }
                    // Get Calendario (Lista)
                    path.contains("${BaseEndpoints.CALENDAR_API}/") && request.method == "GET" -> {
                        val idMatchCancelled = "2b2b2b2b-0000-1111-3333-000000000008"

                        if (isMatchCancelled) {
                            MockResponse().setResponseCode(200).setBody("[]")
                        } else {

                            val isFiltering = url?.queryParameterNames?.isNotEmpty() == true
                            if (isFiltering) {
                                MockResponse().setResponseCode(200).setBody("[$matchToCancelJson]")
                            } else {
                                MockResponse().setResponseCode(200).setBody(calendarJson)
                            }
                        }
                    }
                    else -> MockResponse().setResponseCode(404)
                }
            }
        }

        // INJECT DEPOIS DE CONFIGURAR TUDO
        hiltRule.inject()
    }

    @After
    fun tearDown() {
        mockWebServer.shutdown()
    }

    @Test
    fun testNavigation_WhenNotAuthenticatedUser_ShouldOpenLogin() {
        val context = composeRule.activity.applicationContext
        val jsonLogin = JSONObject(loginBodyJson)

        navigateNotAuthenticatedUserToHomePageTeam(context = context)

        loginAutenticationAdmin(
            context = context,
            email = jsonLogin.getString("email"),
            password = jsonLogin.getString("password")
        )

        verifyHomePageTeamLoad(context = context)

        navigateToCalendarFromHomePage(context = context)

        findMatchToCancelCalendar(context = context)

        cancelMatch(
            context = context,
            description = "Desistimos de realizar a partida"
        )

        validateMatchLeaveCalendar(context = context)
    }

    private fun navigateNotAuthenticatedUserToHomePageTeam(context: Context) {
        composeRule
            .onAllNodesWithText(context.getString(Routes.GeralRoutes.HOMEPAGE.labelResId)) //Titulo da página
            .onFirst()
            .assertIsDisplayed()

        composeRule
            .onNodeWithText(
                context.getString(
                    R.string.welcome_app,
                    context.getString(R.string.user)
                )
            )
            .assertIsDisplayed()

        composeRule
            .onNodeWithText(context.getString(Routes.TeamRoutes.HOMEPAGE.labelResId))
            .performClick()
    }

    private fun loginAutenticationAdmin(context: Context, email: String, password: String) {
        composeRule
            .onNodeWithText(context.getString(Routes.UserRoutes.LOGIN.labelResId))
            .assertIsDisplayed()

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_email_input))
            .performClick()
            .performTextInput(email)

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_password_input))
            .performClick()
            .performTextInput(password)

        composeRule
            .onAllNodesWithTag(context.getString(R.string.tag_login_button))
            .onFirst()
            .performClick()
    }

    private fun verifyHomePageTeamLoad(context: Context) {
        composeRule
            .onAllNodesWithText(context.getString(Routes.TeamRoutes.HOMEPAGE.labelResId))
            .onFirst()
            .assertIsDisplayed()

        composeRule.waitUntil(timeoutMillis = 10000) {
            composeRule
                .onAllNodesWithTag(context.getString(R.string.tag_logo_team))
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_name_team))
            .assertIsDisplayed()
    }

    private fun navigateToCalendarFromHomePage(context: Context) {
        composeRule
            .onNodeWithTag(context.getString(R.string.tag_action_card_calendar))
            .performClick()
    }

    private fun findMatchToCancelCalendar(context: Context) {
        checkThatPageCalendar(context = context)

        filterMatchInCalendar(context = context)

        findMatchAndNavigateToCancelPage(context = context)
    }

    private fun cancelMatch(context: Context, description: String) {
        val tagBotaoDelete = context.getString(R.string.tag_button_cancel_match)
        composeRule.waitUntil(timeoutMillis = 10000) {
            composeRule
                .onAllNodesWithTag(tagBotaoDelete)
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_description_cancel))
            .performClick()
            .performTextInput(description)

        composeRule
            .onNodeWithTag(tagBotaoDelete)
            .performClick()

        composeRule.waitForIdle()
    }

    private fun validateMatchLeaveCalendar(context: Context) {
        checkThatPageCalendar(context = context)

        filterMatchInCalendar(context = context)

        validateMatchIsDeletedOfCalendar(context = context)
    }

    private fun validateMatchIsDeletedOfCalendar(context: Context) {
        val emptyListMessage = context.getString(R.string.list_calendar_empty)

        composeRule.waitUntil(timeoutMillis = 5000) {
            composeRule
                .onAllNodesWithText(emptyListMessage)
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithText(emptyListMessage)
            .assertIsDisplayed()
    }

    //Support Methods
    private fun checkThatPageCalendar(context: Context) {
        val tituloCalendar = context.getString(Routes.TeamRoutes.CALENDAR.labelResId)

        composeRule.waitUntil(timeoutMillis = 10000) {
            composeRule
                .onAllNodesWithText(tituloCalendar)
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onAllNodesWithText(tituloCalendar)
            .onFirst()
            .assertIsDisplayed()
    }

    private fun filterMatchInCalendar(context: Context) {
        val matchJsonObject = JSONObject(matchToCancelJson)

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_filter_section))
            .performClick()

        composeRule.waitUntil(timeoutMillis = 2000) {
            composeRule
                .onAllNodesWithTag(context.getString(R.string.tag_filter_name_team)) // Ou a tag que usas no input de nome
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_filter_name_team))
            .performTextInput(matchJsonObject.getJSONObject("opponent").getString("name"))

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_filter_home_match))
            .performClick()

        val textHome = context.getString(R.string.filter_home)

        composeRule.waitUntil(timeoutMillis = 2000) {
            composeRule
                .onAllNodesWithText(textHome)
                .fetchSemanticsNodes()
                .size > 1
        }

        composeRule
            .onAllNodesWithText(textHome)
            .onLast()
            .performClick()

        composeRule.waitForIdle()

        //TypeMatch
        composeRule
            .onNodeWithTag(context.getString(R.string.tag_filter_type_match))
            .performClick()

        val textCasual = context.getString(TypeMatch.CASUAL.stringId)

        composeRule
            .onAllNodesWithText(textCasual)
            .onLast()
            .performClick()

        composeRule.waitForIdle()

        //IsFinish
        composeRule
            .onNodeWithTag(context.getString(R.string.tag_filter_finish_match))
            .performClick()

        val textScheduled = context.getString(R.string.match_status_scheduled)

        composeRule
            .onAllNodesWithText(textScheduled)
            .onLast()
            .performClick()

        composeRule.waitForIdle()

        //Filtrar
        composeRule
            .onNodeWithTag(context.getString(R.string.tag_apply_filter))
            .performClick()
    }

    private fun findMatchAndNavigateToCancelPage(context: Context) {
        val matchJsonObject = JSONObject(matchToCancelJson)
        val opponentName = matchJsonObject.getJSONObject("opponent").getString("name")

        composeRule.waitUntil(timeoutMillis = 5000) {
            composeRule.onAllNodesWithText(opponentName).fetchSemanticsNodes().isNotEmpty()
        }

        composeRule
            .onAllNodesWithTag(context.getString(R.string.tag_options_match))
            .onFirst()
            .performClick()

        composeRule.mainClock.advanceTimeBy(500)
        composeRule.waitForIdle()

        composeRule
            .onNodeWithText(
                context.getString(R.string.button_cancel_match),
                substring = true,
                ignoreCase = true
            )
            .assertIsDisplayed()
            .performClick()
        composeRule.waitForIdle()
    }
}