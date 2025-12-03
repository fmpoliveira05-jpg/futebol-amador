package com.example.amfootball.data.enums.settings

/**
 * Enumeração que define os idiomas suportados para localização na aplicação.
 *
 * Cada constante associa um nome amigável a um código de localidade (Locale Code)
 * padrão internacional (IETF language tag).
 *
 * @property code O código de localidade (String) usado para configurar o idioma da app
 * no armazenamento local ([SharedPreferences]) e no contexto do Android.
 */
enum class AppLanguage(val code: String) {
    /**
     * Idioma: Inglês. Código de localidade padrão "en".
     */
    ENGLISH("en"),
    /**
     * Idioma: Português de Portugal. Código de localidade específico "pt-PT".
     */
    PORTUGUESE("pt-PT");
}
