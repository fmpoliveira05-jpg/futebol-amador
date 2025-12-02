package com.example.amfootball.systemTests

import androidx.compose.ui.test.junit4.createAndroidComposeRule
import com.example.amfootball.MainActivity
import com.example.amfootball.data.network.interfaces.BaseEndpoints
import com.example.amfootball.utils.JsonReader
import dagger.hilt.android.testing.HiltAndroidRule
import okhttp3.mockwebserver.Dispatcher
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import okhttp3.mockwebserver.RecordedRequest
import org.junit.After
import org.junit.Before
import org.junit.Rule

class NegociateMatchInviteTest {

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
}