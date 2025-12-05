package com.example.amfootball.systemTests

import android.content.Context
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onFirst
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextInput
import androidx.test.rule.GrantPermissionRule
import com.example.amfootball.MainActivity
import com.example.amfootball.R
import com.example.amfootball.navigation.objects.Routes
import dagger.hilt.android.testing.HiltAndroidRule
import dagger.hilt.android.testing.HiltAndroidTest
import org.junit.Rule
import org.junit.Test
import android.Manifest
import androidx.compose.ui.test.ComposeTimeoutException
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onRoot
import androidx.compose.ui.test.printToLog
import com.example.amfootball.data.local.SessionManager
import org.junit.Before
import javax.inject.Inject

@HiltAndroidTest
class SendMessageTest {

    @get:Rule(order = 0)
    val hiltRule = HiltAndroidRule(this)

    @get:Rule(order = 1)
    val composeRule = createAndroidComposeRule<MainActivity>()

    @get:Rule(order = 2)
    val permissionRule: GrantPermissionRule = GrantPermissionRule.grant(
        Manifest.permission.POST_NOTIFICATIONS
    )

    @Inject
    lateinit var sessionManager: SessionManager

    @Before
    fun setUp() {
        hiltRule.inject()

        sessionManager.clearSession()
    }

    @Test
    fun testSendMessageInChat() {
        composeRule.waitForIdle()

        val context = composeRule.activity.applicationContext
        val email = "player@player.com"
        val password = "Teste123."
        val message = "Mensagem de Teste"

        checkHomePageAndNavigateChatList(context = context)

        login(
            context = context,
            email = email,
            password = password
        )

        findAndSelectChat(context = context)

        sendMessage(context = context, message)

        checkMessageAreSend(context = context, message)
    }

    private fun checkHomePageAndNavigateChatList(context: Context) {
        composeRule
            .onAllNodesWithText(context.getString(Routes.GeralRoutes.HOMEPAGE.labelResId))
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
            .onNodeWithText(context.getString(Routes.BottomNavBarRoutes.CHAT_LIST.labelResId))
            .performClick()
    }

    private fun login(context: Context,
                      email: String,
                      password: String
    ) {
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

        composeRule.waitForIdle()
    }

    private fun findAndSelectChat(context: Context) {
        composeRule.waitForIdle()

        composeRule
            .onAllNodesWithText(context.getString(Routes.PlayerRoutes.CHAT_LIST.labelResId))
            .onFirst()
            .assertIsDisplayed()

        val chatItemTag = context.getString(R.string.tag_item_list_chat)
        val emptyStateTag = context.getString(R.string.tag_empty_list_chat)

        try {
            composeRule.waitUntil(timeoutMillis = 10000) {
                composeRule
                    .onAllNodesWithTag(chatItemTag)
                    .fetchSemanticsNodes()
                    .isNotEmpty()
            }

            composeRule.onAllNodesWithTag(chatItemTag)
                .onFirst()
                .performClick()
        } catch (e: ComposeTimeoutException) {
            val emptyStateNode = composeRule.onAllNodesWithTag(emptyStateTag)

            if (emptyStateNode
                .fetchSemanticsNodes()
                .isNotEmpty()
            ) {
                throw AssertionError("FALHA DE TESTE: O utilizador não tem conversas iniciadas. Por favor, cria uma conversa manualmente ou via API antes de correr este teste.")
            } else {
                println("DEBUG: Árvore de UI no momento do erro:")
                composeRule.onRoot().printToLog("ARVORE_UI_ERRO")
                throw e
            }
        }

        composeRule.waitForIdle()
    }

    private fun sendMessage(
        context: Context,
        message: String
    ) {
        composeRule
            .onNodeWithTag(context.getString(R.string.tag_field_message))
            .performClick()
            .performTextInput(message)

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_button_send_message))
            .performClick()
    }

    private fun checkMessageAreSend(context: Context, message: String) {
        composeRule.waitForIdle()

        composeRule
            .onNodeWithText(message)
            .assertIsDisplayed()
    }
}