package com.example.amfootball.core.utils

import com.google.i18n.phonenumbers.PhoneNumberUtil
import java.util.Locale

/**
 * Modelo de dados que representa as informações essenciais de um país para o seletor de telefone.
 *
 * Utilizado para preencher listas de seleção em componentes como o [PhoneInputWithDynamicCountries].
 *
 * @property code O código de região ISO 3166-1 alpha-2 (ex: "PT", "US", "BR").
 * @property name O nome de exibição do país localizado (ex: "Portugal").
 * @property dialCode O prefixo de discagem internacional formatado (ex: "+351").
 * @property flagEmoji O Emoji da bandeira correspondente ao país (ex: "🇵🇹").
 */
data class CountryData(
    val code: String,
    val name: String,
    val dialCode: String,
    val flagEmoji: String
)

/**
 * Objeto utilitário responsável por gerar e fornecer dados sobre países e códigos telefónicos.
 *
 * Utiliza a biblioteca [PhoneNumberUtil] da Google para obter os códigos de discagem corretos
 * e a classe [Locale] do Java para obter os nomes dos países e códigos ISO.
 */
object CountryCodeHelper {

    /**
     * Gera uma lista completa de [CountryData] para todos os países suportados pelo sistema.
     *
     * O processo inclui:
     * 1. Iterar sobre todos os códigos de região ISO disponíveis.
     * 2. Validar se o país possui um código de discagem telefónica válido via [PhoneNumberUtil].
     * 3. Obter o nome do país no idioma atual do dispositivo via [Locale].
     * 4. Gerar o Emoji da bandeira dinamicamente.
     *
     * @return Uma lista de [CountryData] ordenada alfabeticamente pelo nome do país.
     */
    fun getCountries(): List<CountryData> {
        val phoneUtil = PhoneNumberUtil.getInstance()
        val countries = mutableListOf<CountryData>()

        for (regionCode in Locale.getISOCountries()) {
            val dialCode = try {
                phoneUtil.getCountryCodeForRegion(regionCode)
            } catch (e: Exception) {
                continue
            }

            if (dialCode == 0) {
                continue
            }

            val locale = Locale("", regionCode)
            val name = locale.displayCountry
            val flag = countryCodeToEmojiFlag(regionCode)

            countries.add(CountryData(regionCode, name, "+$dialCode", flag))
        }

        return countries.sortedBy { it.name }
    }

    /**
     * Converte um código de região ISO 3166-1 alpha-2 (ex: "PT") num Emoji de bandeira (ex: 🇵🇹).
     *
     * A lógica baseia-se na conversão dos caracteres ASCII para "Regional Indicator Symbols" do Unicode.
     * O offset 0x1F1E6 é a distância entre o 'A' (ASCII) e o primeiro Regional Indicator Symbol 🇦.
     *
     * @param countryCode O código do país em maiúsculas (ex: "US").
     * @return Uma String contendo o Emoji da bandeira.
     */
    private fun countryCodeToEmojiFlag(countryCode: String): String {
        val firstLetter = Character.codePointAt(countryCode, 0) - 0x41 + 0x1F1E6
        val secondLetter = Character.codePointAt(countryCode, 1) - 0x41 + 0x1F1E6
        return String(Character.toChars(firstLetter)) + String(Character.toChars(secondLetter))
    }
}