package com.example.amfootball.ui.components.Buttons

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController

@Composable
fun BackButton(
    navController: NavHostController,
    modifier: Modifier = Modifier
) {
    IconButton(
        onClick = { navController.navigateUp() }, // Ação padrão: voltar na pilha de navegação
        modifier = modifier
    ) {
        Icon(
            imageVector = Icons.AutoMirrored.Filled.ArrowBack, // Ícone padrão de "Voltar"
            contentDescription = "Voltar" // Importante para acessibilidade
            // contentDescription = stringResource(id = R.string.back_button_description) // Melhor ainda: usar recurso de string
        )
    }
}