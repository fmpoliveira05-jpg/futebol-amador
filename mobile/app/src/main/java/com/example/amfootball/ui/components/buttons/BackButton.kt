package com.example.amfootball.ui.components.buttons

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.navigation.NavHostController
import com.example.amfootball.R

/**
 * Um componente Composable que exibe um botão de seta para trás (back button).
 *
 * Ao ser clicado, este botão chama [NavHostController.navigateUp] para retornar
 * ao destino anterior na pilha de navegação (back stack). O ícone é [Icons.AutoMirrored.Filled.ArrowBack],
 * que garante o correto espelhamento em layouts RTL (Right-to-Left).
 *
 * @param navController O controlador de navegação usado para gerenciar a navegação,
 * especificamente para chamar o método [navigateUp].
 * @param modifier Um [Modifier] opcional para configurar a aparência ou o comportamento do [IconButton].
 */
@Composable
fun BackButton(
    navController: NavHostController,
    modifier: Modifier = Modifier
) {
    IconButton(
        onClick = { navController.navigateUp() },
        modifier = modifier
    ) {
        Icon(
            imageVector = Icons.AutoMirrored.Filled.ArrowBack,
            contentDescription = stringResource(id = R.string.button_back_description)
        )
    }
}

