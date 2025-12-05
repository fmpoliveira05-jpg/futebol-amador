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
import androidx.compose.ui.test.onAllNodesWithTag
import com.example.amfootball.data.local.SessionManager
import org.junit.Before
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

        checkHomePageAndNavigateChatList(context = context)

        login(
            context = context,
            email = email,
            password = password
        )

        findAndSelectChat(context = context)

        sendMessage(context = context, testMessage)

        checkMessageAreSend(context = context)
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

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_login_button))
            .performClick()
    }

    private fun findAndSelectChat(context: Context) {
        composeRule
            .onAllNodesWithText(context.getString(Routes.PlayerRoutes.CHAT_LIST.labelResId))
            .onFirst()
            .assertIsDisplayed()

        val chatItemTag = context.getString(R.string.tag_item_list_chat)

        composeRule.waitUntil(timeoutMillis = 10000) {
            composeRule
                .onAllNodesWithTag(chatItemTag)
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onAllNodesWithTag(chatItemTag)
            .onFirst()
            .performClick()

        composeRule.waitForIdle()
    }

    private fun sendMessage(context: Context, message: String) {
        //TODO: Ir buscar o titulo do chat

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_field_message))
            .performClick()
            .performTextInput(message)

        composeRule
            .onNodeWithTag(context.getString(R.string.tag_button_send_message))
        
    }

    private fun checkMessageAreSend(context: Context) {

    }
}