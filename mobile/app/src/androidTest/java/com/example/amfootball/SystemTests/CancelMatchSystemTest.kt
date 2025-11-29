package com.example.amfootball.SystemTests

import androidx.activity.compose.setContent
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onNodeWithText
import com.example.amfootball.MainActivity
import com.example.amfootball.navigation.MainNavigation
import com.example.amfootball.ui.theme.AMFootballTheme
import org.junit.Before
import org.junit.Rule
import org.junit.Test

class CancelMatchSystemTest {
    //Permite definir regras
    @get:Rule
    val composeRule = createAndroidComposeRule<MainActivity>()

    //Executa antes de cada teste
    @Before
    fun setUp() {
        composeRule.activity.setContent {
            AMFootballTheme {
                MainNavigation()
            }
        }
    }

    //Testa a navegação entre páginas
    @Test
    fun testNavigationAmongPages() {
        val context = composeRule.activity.applicationContext

        //Verificar se estamos na homePage
        composeRule.onNodeWithText(context.getString())
    }
}