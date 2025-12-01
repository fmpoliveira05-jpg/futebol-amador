package com.example.amfootball.systemTests

import android.content.Context
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onFirst
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextInput
import com.example.amfootball.MainActivity
import com.example.amfootball.data.network.interfaces.BaseEndpoints
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.JsonReader
import dagger.hilt.android.testing.HiltAndroidRule
import dagger.hilt.android.testing.HiltAndroidTest
import okhttp3.mockwebserver.Dispatcher
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import okhttp3.mockwebserver.RecordedRequest
import org.json.JSONObject
import org.junit.After
import org.junit.Before
import org.junit.Rule
import org.junit.Test
import com.example.amfootball.R

@HiltAndroidTest
class CancelMatchSystemTest {
    @get:Rule(order = 0)
    val hiltRule = HiltAndroidRule(this)
    @get:Rule(order = 1)
    val composeRule = createAndroidComposeRule<MainActivity>()

    private lateinit var mockWebServer: MockWebServer

    private val loginBodyJson = JsonReader.readJsonFromAsserts("LoginAdminTeam.json")
    private val loginResponseJson = JsonReader.readJsonFromAsserts("ProfileAdmin.json")
    private val calendarJson = JsonReader.readJsonFromAsserts("CalendarOfTeam.json")
    private val detailsTeamJson = JsonReader.readJsonFromAsserts("DetailsTeam.json")
    private val matchToCancelJson = JsonReader.readJsonFromAsserts("MatchToCancel.json")

    @Before
    fun setUp() {
        hiltRule.inject()

        mockWebServer = MockWebServer()
        mockWebServer.start(8080)

        //TODO: Meter o endPoint de filtrar
        mockWebServer.dispatcher = object : Dispatcher() {
            override fun dispatch(request: RecordedRequest): MockResponse {
                val path = request.path ?: ""

                return when {
                    //Pedido do login
                    path.contains("${BaseEndpoints.authApi}/login") && request.method == "POST" -> {
                        MockResponse().setResponseCode(200).setBody(loginResponseJson)
                    }

                    //Pedido da HomePage
                    path.contains("${BaseEndpoints.teamApi}/opponent/") && request.method == "GET" -> {
                        MockResponse().setResponseCode(200).setBody(detailsTeamJson)
                    }

                    //Pedido do calendario
                    path.contains("${BaseEndpoints.teamApi}/") && request.method == "GET" -> {
                        MockResponse().setResponseCode(200).setBody(calendarJson)
                    }

                    //Pedido para ir buscar detalhes da partida
                    path.contains("${BaseEndpoints.calendarApi}/") && request.method == "GET" && path.count { it == '/' } >= 4 -> {
                        MockResponse().setResponseCode(200).setBody(matchToCancelJson)
                    }

                    //Pedido de cancelamento
                    path.contains("${BaseEndpoints.calendarApi}/") && path.contains("/CancelMatch/") && request.method == "DELETE" -> {
                        val descriptionRecebida = request.body.readUtf8()
                        println("A App tentou cancelar com descrição: $descriptionRecebida")

                        return MockResponse().setResponseCode(204)
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

    //Testa a navegação entre páginas
    @Test
    fun testNavigation_WhenNotAuthenticatedUser_ShouldOpenLogin() {
        val context = composeRule.activity.applicationContext
        val jsonLogin = JSONObject(loginBodyJson)

        NavigateNotAuthenticatedUserToHomePageTeam(context = context)

        LoginAutenticationAdmin(
            context = context,
            email = jsonLogin.getString("email"),
            password = jsonLogin.getString("password")
        )

        VerifyHomePageTeamLoad(context = context)

        NavigateToCalendarFromHomePage(context = context)

        //TODO: Falta implementar
        FindMatchToCancelCalendar(context = context)

        //TODO: Implementar
        CancelMatch(context = context, description = "")

        ValidateMatchLeaveCalendar(context = context)
    }

    private fun NavigateNotAuthenticatedUserToHomePageTeam(context: Context) {
        composeRule
            .onAllNodesWithText(context.getString(Routes.GeralRoutes.HOMEPAGE.labelResId)) //Titulo da página
            .onFirst()
            .assertIsDisplayed()

        composeRule
            .onNodeWithText(context.getString(R.string.welcome_app, context.getString(R.string.user)))
            .assertIsDisplayed()

        composeRule
            .onNodeWithText(context.getString(Routes.TeamRoutes.HOMEPAGE.labelResId))
            .performClick()
    }

    private fun LoginAutenticationAdmin(context: Context, email: String, password: String) {
        //Validação se estamos na página
        composeRule
            .onNodeWithText(context.getString(Routes.UserRoutes.LOGIN.labelResId))
            .assertIsDisplayed()

        //Login
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

    private fun VerifyHomePageTeamLoad(context: Context) {
        composeRule
            .onAllNodesWithText(context.getString(Routes.TeamRoutes.HOMEPAGE.labelResId)) //Titulo da página
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

    private fun NavigateToCalendarFromHomePage(context: Context) {
        composeRule
            .onNodeWithText(context.getString(Routes.BottomNavBarRoutes.PAGE_OPTIONS.labelResId))
            .performClick()

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_action_card_calendar))
            .performClick()
    }

    /*
    *    private fun NavigateToCalendarFromNavBar(context: Context) {
        composeRule
            .onNodeWithText(context.getString(Routes.BottomNavBarRoutes.PAGE_OPTIONS.labelResId))
            .performClick()

        composeRule
            .onNodeWithText(context.getString(Routes.TeamRoutes.CALENDAR.labelResId))
            .performClick()
    }
    * */

    //TODO:Implementar
    private fun FindMatchToCancelCalendar(context: Context) {
        //TODO: Meter para filtrar pela partida e cancelar
        /*
        *         composeRule
            .onAllNodesWithText(context.getString(Routes.TeamRoutes.CALENDAR.labelResId)) //Titulo da página
            .onFirst()
            .assertIsDisplayed()
        * */
    }

    //TODO: Implementar
    private fun CancelMatch(context: Context, description: String) {
    }

    //TODO: Implementar
    private fun ValidateMatchLeaveCalendar(context: Context) {

    }
}