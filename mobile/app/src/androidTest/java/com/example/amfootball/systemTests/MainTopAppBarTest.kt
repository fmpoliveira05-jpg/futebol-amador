package com.example.amfootball.systemTests

import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import com.example.amfootball.MainActivity
import dagger.hilt.android.testing.HiltAndroidRule
import dagger.hilt.android.testing.HiltAndroidTest
import org.junit.Before
import org.junit.Rule
import org.junit.Test

@HiltAndroidTest
class MainTopAppBarTest {

    @get:Rule(order = 0)
    val hiltRule = HiltAndroidRule(this)

    @get:Rule(order = 1)
    val composeRule = createAndroidComposeRule<MainActivity>()

    @Before
    fun setup() {
        hiltRule.inject()
    }

    @Test
    fun clickingLoginButton_navigatesToLoginScreen() {
        composeRule
            .onNodeWithTag("loginButton")
            .assertExists()
            .performClick()

        composeRule
            .onNodeWithText("Login")
            .assertExists()
    }

    @Test
    fun clickingLogin_navigatesToLogin() {
        composeRule
            .onNodeWithTag("loginButton")
            .assertExists()
            .performClick()

        composeRule
            .onNodeWithText("Login")
            .assertExists()
    }



}
