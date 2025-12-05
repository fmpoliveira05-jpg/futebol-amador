package com.example.amfootball.systemTests

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
import com.example.amfootball.MainActivity
import com.example.amfootball.R
import com.example.amfootball.data.enums.match.TypeMatch
import com.example.amfootball.data.network.interfaces.BaseEndpoints
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.JsonReader
import dagger.hilt.android.testing.HiltAndroidRule
import dagger.hilt.android.testing.HiltAndroidTest
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

@HiltAndroidTest
class CancelMatchSystemTest {
    @get:Rule(order = 0)
    val hiltRule = HiltAndroidRule(this)

    @get:Rule(order = 1)
    val composeRule = createAndroidComposeRule<MainActivity>()

    private lateinit var mockWebServer: MockWebServer
    private lateinit var loginBodyJson: String
    private lateinit var loginResponseJson: String
    private lateinit var calendarJson: String
    private lateinit var detailsTeamJson: String
    private lateinit var matchToCancelJson: String
    private lateinit var dynamicExpectedDate: String

    @Before
    fun setUp() {
        hiltRule.inject()

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
        var isMatchCancelled = false

        mockWebServer = MockWebServer()
        mockWebServer.start(8080)

        mockWebServer.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse {
                val path = request.path ?: ""
                val url = request.requestUrl

                return when {
                    //Pedido do login
                    path.contains("${BaseEndpoints.AUTH_API}/login") && request.method == "POST" -> {
                        MockResponse().setResponseCode(200).setBody(loginResponseJson)
                    }

                    //Dados da HomePage
                    path.contains("${BaseEndpoints.TEAM_API}/opponent/") && request.method == "GET" -> {
                        MockResponse().setResponseCode(200).setBody(detailsTeamJson)
                    }

                    // Cancelar Partida
                    path.contains("${BaseEndpoints.CALENDAR_API}/") && path.contains("/CancelMatch/") && request.method == "DELETE" -> {
                        isMatchCancelled = true
                        MockResponse().setResponseCode(204)
                    }

                    // Detalhes da Partida, antes de cancelar
                    path.contains("${BaseEndpoints.CALENDAR_API}/") && request.method == "GET" && path.count { it == '/' } >= 4 -> {
                        MockResponse().setResponseCode(200).setBody(matchToCancelJson)
                    }

                    //Get Calendario
                    path.contains("${BaseEndpoints.CALENDAR_API}/") && request.method == "GET" -> {
                        val idMatchCancelled = "2b2b2b2b-0000-1111-3333-000000000008"

                        val listaBase: String = if (isMatchCancelled) {
                            val jsonArrayOriginal = JSONArray(calendarJson)
                            val jsonArrayNovo = JSONArray()

                            for (i in 0 until jsonArrayOriginal.length()) {
                                val matchItem = jsonArrayOriginal.getJSONObject(i)
                                if (matchItem.getString("idMatch") != idMatchCancelled) {
                                    jsonArrayNovo.put(matchItem)
                                }
                            }
                            jsonArrayNovo.toString()
                        } else {
                            calendarJson
                        }

                        val isFiltering = url?.queryParameterNames?.isNotEmpty() == true

                        if (isFiltering) {
                            if (isMatchCancelled) {
                                MockResponse().setResponseCode(200).setBody("[]")
                            } else {
                                MockResponse().setResponseCode(200).setBody("[$matchToCancelJson]")
                            }
                        } else {
                            MockResponse().setResponseCode(200).setBody(listaBase)
                        }
                    }

                    else -> MockResponse().setResponseCode(404)
                }
            }
        }
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
            .onNodeWithTag(context.getString(R.string.tag_login_button))
            .performClick()
    }

    private fun verifyHomePageTeamLoad(context: Context) {
        composeRule
            .onAllNodesWithText(context.getString(Routes.TeamRoutes.HOMEPAGE.labelResId))
            .onFirst()
            .assertIsDisplayed()

        composeRule.waitUntil(timeoutMillis = 5000) {
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
        composeRule.waitUntil(timeoutMillis = 5000) {
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