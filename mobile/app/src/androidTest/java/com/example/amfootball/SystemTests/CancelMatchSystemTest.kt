package com.example.amfootball.SystemTests

import androidx.activity.compose.setContent
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import com.example.amfootball.MainActivity
import com.example.amfootball.navigation.MainNavigation
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.theme.AMFootballTheme
import dagger.hilt.android.testing.HiltAndroidRule
import dagger.hilt.android.testing.HiltAndroidTest
import org.junit.Before
import org.junit.Rule
import org.junit.Test

@HiltAndroidTest
class CancelMatchSystemTest {
    //Permite definir regras
    @get:Rule(order = 0)
    val hiltRule = HiltAndroidRule(this)
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
    fun testNavigation_WhenNotAuthenticatedUser_ShouldOpenLogin() {
        val context = composeRule.activity.applicationContext

        //Procuramos o titulo da página
        composeRule
            .onNodeWithText(context.getString(Routes.GeralRoutes.HOMEPAGE.labelResId)) //Titulo da página
            .assertIsDisplayed()

        //Procuramos o botão de homePage da team(que vai levar para o login)
        composeRule
            .onNodeWithText(context.getString(Routes.TeamRoutes.HOMEPAGE.labelResId))
            .performClick()


        //Encontrar a strings do botão
        //TODO: Sem aut ao clicar em teamPage leva para o login
        //TODO: Procurar o botão e clicar nele
    }
}