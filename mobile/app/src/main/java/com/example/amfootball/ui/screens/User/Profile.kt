package com.example.amfootball.ui.screens.User

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.ui.components.BackTopBar
import com.example.amfootball.ui.theme.AMFootballTheme

@Composable
fun ProfileScreen(navController: NavHostController) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navController,
                title = stringResource(id = R.string.page_profile_user)
            )
        },
        content = { paddingValues ->
            ProfileScreenContent(
                modifier = Modifier
                    .padding(paddingValues)
                    .padding(16.dp)
            )
        }
    )
}

@Composable
private fun ProfileScreenContent(modifier: Modifier = Modifier) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Text(text = stringResource(id = R.string.profile_user_title_page))

        TextFieldProfile()
    }
}

@Composable
private fun TextFieldProfile() {
    CreateTextField(label = "Nome",
        value = "Joaquim Ferreira",
        isSingleLine = true)

    CreateTextField(label = "Data de nascimento",
        value = "11/09/2001",
        isSingleLine = true)

    CreateTextField(label = "Morada",
        value = "King Fahd Road, Riyadh",
        isSingleLine = false)

    CreateTextField(label = "Posição",
        value = "Atacante",
        isSingleLine = true)

    CreateTextField(label = "Altura",
        value = "180 cm",
        isSingleLine = true)

    CreateTextField(label = "Equipa",
        value = "Predadores de Pereca",
        isSingleLine = true)
}

@Composable
private fun CreateTextField(label: String,
                            value: String,
                            isSingleLine: Boolean) {
    Column() {
        Text(text = "$label:")
        Spacer(Modifier.height(4.dp))
        TextField(
            label = { Text(text = label) },
            value = value,
            onValueChange = {},
            singleLine = isSingleLine,
            readOnly = true,
        )
    }
}

@Preview(name = "Preview Profile Screen Eng",
        locale = "en",
        showBackground = true)
@Preview(name = "Preview Profile Screen PT",
    locale = "pt",
    showBackground = true)
@Composable
fun ProfileScreenPreview() {
    AMFootballTheme {
        ProfileScreen(navController = rememberNavController())
    }
}