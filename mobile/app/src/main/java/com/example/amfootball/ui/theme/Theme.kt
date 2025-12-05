package com.example.amfootball.ui.theme

import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.dynamicDarkColorScheme
import androidx.compose.material3.dynamicLightColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext

/**
 * Esquema de cores padrão para o Modo Escuro (Dark Theme).
 *
 * Utilizado em dispositivos Android mais antigos (< Android 12) ou quando a cor dinâmica está desativada.
 */
private val DarkColorScheme = darkColorScheme(
    primary = Purple80,
    secondary = PurpleGrey80,
    tertiary = Pink80
)

/**
 * Esquema de cores padrão para o Modo Claro (Light Theme).
 *
 * Utilizado em dispositivos Android mais antigos (< Android 12) ou quando a cor dinâmica está desativada.
 */
private val LightColorScheme = lightColorScheme(
    primary = Purple40,
    secondary = PurpleGrey40,
    tertiary = Pink40

    /* Other default colors to override
    background = Color(0xFFFFFBFE),
    surface = Color(0xFFFFFBFE),
    onPrimary = Color.White,
    onSecondary = Color.White,
    onTertiary = Color.White,
    onBackground = Color(0xFF1C1B1F),
    onSurface = Color(0xFF1C1B1F),
    */
)

/** Cor Primária do Google (Azul). */
val GoogleBlue = Color(0xFF4285F4)

/** Cor Secundária do Google (Branco). */
val GoogleWhite = Color(0xFFFFFFFF)

/** Cor Primária do Facebook (Azul). */
val FacebookBlue = Color(0xFF1877F2)

/** Cor Secundária do Facebook (Branco). */
val FacebookWhite = Color(0xFFFFFFFF)

/**
 * Componente Composable que aplica o tema Material 3 à aplicação.
 *
 * Determina o esquema de cores ([ColorScheme]) a ser usado com base na lógica:
 * 1. Cor Dinâmica (Material You) no Android 12+ (Build.VERSION_CODES.S).
 * 2. Esquema de cores padrão (DarkColorScheme ou LightColorScheme).
 *
 * @param darkTheme Determina se o tema deve ser escuro. O padrão é a preferência do sistema ([isSystemInDarkTheme]).
 * @param dynamicColor Determina se deve ser usada a cor dinâmica do sistema (Material You). Padrão é true.
 * @param content O conteúdo Composable ao qual este tema será aplicado.
 */
@Composable
fun AMFootballTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    // Dynamic color is available on Android 12+
    dynamicColor: Boolean = true,
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        dynamicColor && Build.VERSION.SDK_INT >= Build.VERSION_CODES.S -> {
            val context = LocalContext.current
            if (darkTheme) dynamicDarkColorScheme(context) else dynamicLightColorScheme(context)
        }

        darkTheme -> DarkColorScheme
        else -> LightColorScheme
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content
    )
}