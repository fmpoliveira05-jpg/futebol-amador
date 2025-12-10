package com.example.amfootball.systemTests

import android.Manifest
import android.content.Context
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithTag
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onFirst
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performScrollTo
import androidx.compose.ui.test.performTextInput
import androidx.test.rule.GrantPermissionRule
import com.example.amfootball.MainActivity
import com.example.amfootball.R
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.ui.navigation.objects.Routes
import dagger.hilt.android.testing.HiltAndroidRule
import dagger.hilt.android.testing.HiltAndroidTest
import org.junit.Before
import org.junit.Rule
import org.junit.Test
import javax.inject.Inject

@HiltAndroidTest
class SendMessageTest {
    val testMessage = "Test message";
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

        checkMessageAreSend(message)
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

    private fun login(context: Context, email: String, password: String) {
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

        composeRule.waitForIdle()

        composeRule
            .onAllNodesWithTag(context.getString(R.string.tag_login_button))
            .onFirst()
            .performClick()

        composeRule.waitForIdle()
    }

    private fun findAndSelectChat(context: Context) {
        val homeTitle = context.getString(Routes.GeralRoutes.HOMEPAGE.labelResId)
        if (composeRule.onAllNodesWithText(homeTitle).fetchSemanticsNodes().isNotEmpty()) {
            composeRule
                .onNodeWithText(context.getString(Routes.BottomNavBarRoutes.CHAT_LIST.labelResId))
                .performClick()
        }

        val chatItemTag = context.getString(R.string.tag_item_list_chat)

        composeRule.waitUntil(timeoutMillis = 15000) {
            composeRule
                .onAllNodesWithTag(chatItemTag)
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onAllNodesWithTag(chatItemTag)
            .onFirst()
            .performClick()

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

        composeRule.waitForIdle()
    }

    private fun checkMessageAreSend(message: String) {
        composeRule.waitForIdle()

        composeRule
            .onNodeWithText(message)
            .performScrollTo()
            .assertIsDisplayed()
    }
}