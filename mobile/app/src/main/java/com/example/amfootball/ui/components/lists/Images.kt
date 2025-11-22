package com.example.amfootball.ui.components.lists

import android.net.Uri
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.vector.rememberVectorPainter
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage
import androidx.compose.runtime.remember
import kotlin.text.isNullOrEmpty

/**
 * Exibe uma imagem de perfil grande (tamanho padrão: 100.dp).
 *
 * É um wrapper que chama [ImageIcon], encapsulando-o numa [Box] para alinhamento.
 *
 * @param image URI da imagem a ser exibida.
 * @param contentDescription Descrição para acessibilidade.
 * @param modifier Modificador para estilizar a [Box] externa.
 */
@Composable
fun ProfilesImage(
    image: Uri,
    contentDescription: String? = "",
    modifier: Modifier = Modifier
) {
    Box(
        modifier = modifier,
        contentAlignment = Alignment.Center
    ) {
        ImageIcon(
            contentDescription = contentDescription,
            image = image,
            sizeIamge = 100.dp,
        )
    }
}

/**
 * Wrapper para [ProfilesImage] que lida com o input de uma String (URL/caminho).
 *
 * Tenta parsear a String para [Uri]. Se a String for null, vazia, ou o parse falhar,
 * usa [Uri.EMPTY], que acionará o fallback padrão (ícone de círculo).
 *
 * @param image A URL ou caminho da imagem como String (pode ser null).
 * @param contentDescription Descrição para acessibilidade.
 * @param modifier Modificador para estilizar o componente.
 */
@Composable
fun ProfilesImageString(
    image: String?,
    contentDescription: String? = "",
    modifier: Modifier = Modifier
) {
    val imageUri = remember(image) {
        if (image.isNullOrEmpty()) {
            Uri.EMPTY
        } else {
            try {
                Uri.parse(image)
            } catch (e: Exception) {
                Uri.EMPTY
            }
        }
    }

    ProfilesImage(
        image = imageUri,
        contentDescription = contentDescription,
        modifier = modifier
    )
}

/**
 * Exibe uma imagem pequena, adequada para itens de lista (tamanho fixo: 40.dp).
 *
 * Se o [image] for null ou [Uri.EMPTY], exibe um ícone [Icons.Default.AccountCircle]
 * como placeholder em vez de carregar a imagem.
 *
 * @param image URI da imagem a ser exibida.
 */
@Composable
fun ImageList(
    image: Uri?
) {
    if (image != null && image != Uri.EMPTY) {
        ImageIcon(
            image = image,
            contentDescription = "Foto do jogador",
            sizeIamge = 40.dp
        )
    } else {
        Icon(
            imageVector = Icons.Default.AccountCircle,
            contentDescription = "Foto do jogador",
            modifier = Modifier.size(40.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

/**
 * Wrapper para [ImageList] que lida com o input de uma String (URL/caminho).
 *
 * Tenta parsear a String para [Uri]. Se o parsing falhar, usa [Uri.EMPTY]
 * e aciona o ícone de placeholder do [ImageList].
 *
 * @param image A URL ou caminho da imagem como String (pode ser null).
 */
@Composable
fun StringImageList(
    image: String?
) {
    val imageUri = remember(image) {
        if (image.isNullOrEmpty()) {
            Uri.EMPTY
        } else {
            try {
                Uri.parse(image)
            } catch (e: Exception) {
                Uri.EMPTY
            }
        }
    }

    ImageList(image = imageUri)
}

/**
 * Componente base para carregar e exibir imagens em formato circular.
 *
 * Utiliza [AsyncImage] (Coil) para carregamento assíncrono e define [Icons.Default.AccountCircle]
 * como fallback/erro, garantindo que sempre haja um visual.
 *
 * @param contentDescription Descrição para acessibilidade da imagem.
 * @param image URI da imagem a ser carregada.
 * @param sizeIamge Dimensão (Dp) do círculo da imagem.
 */
@Composable
fun ImageIcon(
    contentDescription: String? = "",
    image: Uri,
    sizeIamge: Dp
) {
    val fallbackPainter = rememberVectorPainter(Icons.Default.AccountCircle)

    AsyncImage(
        model = image,
        contentDescription = contentDescription,
        contentScale = ContentScale.Crop,
        fallback = fallbackPainter,
        error = fallbackPainter,
        modifier = Modifier
            .size(sizeIamge)
            .clip(CircleShape),
    )
}