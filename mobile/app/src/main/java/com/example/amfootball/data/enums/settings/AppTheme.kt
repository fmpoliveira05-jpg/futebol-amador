package com.example.amfootball.data.enums.settings

/**
 * Enumeração que define os temas visuais suportados pela aplicação.
 *
 * Esta enumeração é utilizada para gerir a preferência de tema do utilizador ([Light Mode], [Dark Mode], ou Sistema)
 * no armazenamento local, e deve ser usada para controlar o estado da composição do tema ([isSystemInDarkTheme]).
 */
enum class AppTheme {
    /**
     * Aplica o tema de cor claro (Light Mode), independentemente das definições do sistema.
     */
    LIGHT_MODE,

    /**
     * Aplica o tema de cor escuro (Dark Mode), independentemente das definições do sistema.
     */
    DARK_MODE,

    /**
     * O tema visual segue automaticamente o modo Claro/Escuro definido nas configurações globais do sistema operativo.
     */
    SYSTEM_DEFAULT
}