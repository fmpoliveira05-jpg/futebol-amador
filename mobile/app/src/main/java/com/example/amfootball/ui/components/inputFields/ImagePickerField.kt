package com.example.amfootball.ui.components.inputFields

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.PickVisualMediaRequest
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AddAPhoto
import androidx.compose.material3.Icon
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage

/**
 * Componente de seleção de imagens que trabalha com [String].
 * Ideal para interagir com DTOs ou APIs que esperam URLs ou URIs serializadas.
 *
 * Internamente converte a [Uri] selecionada pelo sistema para [String].
 *
 * @param modifier Modificador para estilizar o contentor externo.
 * @param imageSelectedUrl A string representando a imagem (URL ou URI). Se nulo/vazio, mostra placeholder.
 * @param onImageSelected Callback invocado com a string da URI selecionada (ou null).
 * @param contentDescription Descrição para acessibilidade da imagem carregada.
 * @param contentDescriptionWithoutImage Descrição para acessibilidade do ícone de placeholder.
 */
@Composable
fun ImagePickerString(
    modifier: Modifier = Modifier,
    imageSelectedUrl: String?,
    onImageSelected: (String?) -> Unit,
    contentDescription: String? = null,
    contentDescriptionWithoutImage: String? = null,
) {
    val photoPickerLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.PickVisualMedia(),
        onResult = { uri -> onImageSelected(uri?.toString()) }
    )

    BaseImagePickerContent(
        modifier = modifier,
        model = imageSelectedUrl,
        contentDescription = contentDescription,
        contentDescriptionWithoutImage = contentDescriptionWithoutImage,
        onClick = {
            photoPickerLauncher.launch(
                PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly)
            )
        }
    )
}

/**
 * Componente de seleção de imagens que trabalha com objetos [Uri] nativos do Android.
 * Ideal para manipulação de arquivos locais ou upload via ContentResolver antes de serializar.
 *
 * @param modifier Modificador para estilizar o contentor externo.
 * @param imageSelectedUri O [Uri] da imagem. Se nulo, mostra placeholder.
 * @param onImageSelected Callback invocado com o objeto [Uri] selecionado (ou null).
 * @param contentDescription Descrição para acessibilidade da imagem carregada.
 * @param contentDescriptionWithoutImage Descrição para acessibilidade do ícone de placeholder.
 */
@Composable
fun ImagePicker(
    modifier: Modifier = Modifier,
    imageSelectedUri: Uri?,
    onImageSelected: (Uri?) -> Unit,
    contentDescription: String? = null,
    contentDescriptionWithoutImage: String? = null,
) {
    val photoPickerLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.PickVisualMedia(),
        onResult = { uri -> onImageSelected(uri) }
    )

    BaseImagePickerContent(
        modifier = modifier,
        model = imageSelectedUri,
        contentDescription = contentDescription,
        contentDescriptionWithoutImage = contentDescriptionWithoutImage,
        onClick = {
            photoPickerLauncher.launch(
                PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly)
            )
        }
    )
}

/**
 * Componente interno privado que detém toda a lógica visual (UI) para evitar duplicação de código.
 * É agnóstico quanto ao tipo de launcher (Uri ou String), recebendo apenas o modelo para o Coil e a ação de click.
 *
 * @param model O modelo para o Coil carregar (pode ser String, Uri, URL, etc.).
 * @param onClick Ação a ser executada ao clicar no componente (geralmente lançar o picker).
 */
@Composable
private fun BaseImagePickerContent(
    modifier: Modifier,
    model: Any?,
    contentDescription: String?,
    contentDescriptionWithoutImage: String?,
    onClick: () -> Unit
) {
    Box(
        modifier = modifier
            .size(150.dp)
            .clip(CircleShape)
            .background(Color.LightGray.copy(alpha = 0.6f))
            .border(1.dp, Color.Gray, CircleShape)
            .clickable(onClick = onClick),
        contentAlignment = Alignment.Center
    ) {
        val hasImage = when (model) {
            null -> false
            is String -> model.isNotBlank()
            else -> true
        }

        if (hasImage) {
            AsyncImage(
                model = model,
                contentDescription = contentDescription,
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Crop
            )
        } else {
            Icon(
                imageVector = Icons.Default.AddAPhoto,
                contentDescription = contentDescriptionWithoutImage,
                tint = Color.Gray,
                modifier = Modifier.size(48.dp)
            )
        }
    }
}