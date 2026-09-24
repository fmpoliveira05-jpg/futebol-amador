package com.example.amfootball.ui.components.inputFields

import android.net.Uri
import android.widget.Toast
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
import androidx.compose.material.icons.filled.CameraAlt
import androidx.compose.material.icons.filled.PhotoLibrary
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import coil.compose.AsyncImage
import com.example.amfootball.R
import com.example.amfootball.ui.components.camera.CameraCaptureScreen
import android.Manifest

/**
 * Componente de seleção de imagens que trabalha com [String].
 * Ideal para interagir com DTOs ou APIs que esperam URLs ou URIs serializadas.
 *
 * Internamente converte a [Uri] selecionada pelo sistema para [String].
 *
 * @param modifier Modificador para estilizar o contentor externo.
 * @param model A string representando a imagem (URL ou URI). Se nulo/vazio, mostra placeholder.
 * @param onImageSelected Callback invocado com a string da URI selecionada (ou null).
 * @param contentDescription Descrição para acessibilidade da imagem carregada.
 * @param contentDescriptionWithoutImage Descrição para acessibilidade do ícone de placeholder.
 */
@Composable
fun ImagePickerString(
    modifier: Modifier = Modifier,
    model: String?,
    onImageSelected: (String?) -> Unit,
    contentDescription: String? = null,
    contentDescriptionWithoutImage: String? = null,
) {
    ImagePicker(
        modifier = modifier,
        model = model,
        onImageSelected = { uri -> onImageSelected(uri?.toString()) },
        contentDescription = contentDescription,
        contentDescriptionWithoutImage = contentDescriptionWithoutImage
    )
}

/**
 * Componente de seleção de imagens que trabalha com objetos [Uri] nativos do Android.
 * Ideal para manipulação de arquivos locais ou upload via ContentResolver antes de serializar.
 *
 * @param modifier Modificador para estilizar o contentor externo.
 * @param model O modelo para o Coil desenhar (pode ser String URL, Uri, R.drawable, etc).
 * @param onImageSelected Callback invocado com o objeto [Uri] selecionado (ou null).
 * @param contentDescription Descrição para acessibilidade da imagem carregada.
 * @param contentDescriptionWithoutImage Descrição para acessibilidade do ícone de placeholder.
 */
@Composable
fun ImagePicker(
    modifier: Modifier = Modifier,
    model: Any?,
    onImageSelected: (Uri?) -> Unit,
    contentDescription: String? = null,
    contentDescriptionWithoutImage: String? = null,
) {
    val context = LocalContext.current
    var showSourceDialog by remember { mutableStateOf(false) }
    var showCameraScreen by remember { mutableStateOf(false) }

    // 1. Launcher para a Galeria
    val galleryLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.PickVisualMedia(),
        onResult = { uri -> onImageSelected(uri) }
    )

    val permissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission(),
        onResult = { isGranted ->
            if (isGranted) {
                showCameraScreen = true
            } else {
                Toast.makeText(
                    context,
                    context.getString(R.string.permission_camera),
                    Toast.LENGTH_SHORT
                ).show()
            }
        }
    )

    BaseImagePickerContent(
        modifier = modifier,
        model = model,
        contentDescription = contentDescription,
        contentDescriptionWithoutImage = contentDescriptionWithoutImage,
        onClick = { showSourceDialog = true }
    )

    if (showSourceDialog) {
        SourceSelectionDialog(
            onDismiss = { showSourceDialog = false },
            onGalleryClick = {
                showSourceDialog = false
                galleryLauncher.launch(
                    PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly)
                )
            },
            onCameraClick = {
                showSourceDialog = false
                permissionLauncher.launch(Manifest.permission.CAMERA)
            }
        )
    }

    if (showCameraScreen) {
        Dialog(
            onDismissRequest = { showCameraScreen = false },
            properties = DialogProperties(usePlatformDefaultWidth = false)
        ) {
            CameraCaptureScreen(
                onImageCaptured = { uri ->
                    onImageSelected(uri)
                    showCameraScreen = false
                },
                onError = { exception ->
                    Toast.makeText(context, "Erro: ${exception.message}", Toast.LENGTH_SHORT).show()
                    showCameraScreen = false
                }
            )
        }
    }
}
/**
 * Diálogo auxiliar para selecionar a fonte da imagem.
 *
 * @param onDismiss Ação ao fechar o diálogo.
 * @param onGalleryClick Ação ao escolher a galeria.
 * @param onCameraClick Ação ao escolher a câmara.
 */
@Composable
private fun SourceSelectionDialog(
    onDismiss: () -> Unit,
    onGalleryClick: () -> Unit,
    onCameraClick: () -> Unit
) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(text = stringResource(id = R.string.select_image)) },
        text = { Text(text = stringResource(id = R.string.choose_image_source)) },
        confirmButton = {
            TextButton(onClick = onGalleryClick) {
                Icon(Icons.Default.PhotoLibrary, contentDescription = null)
                Text(stringResource(id = R.string.galery))
            }
        },
        dismissButton = {
            TextButton(onClick = onCameraClick) {
                Icon(Icons.Default.CameraAlt, contentDescription = null)
                Text(stringResource(id = R.string.chamber))
            }
        }
    )
}


/**
 * Componente interno privado que detém toda a lógica visual (UI).
 *
 * @param model O modelo para o Coil carregar (pode ser String, Uri, URL, etc.).
 * @param onClick Ação a ser executada ao clicar no componente.
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
            is Uri -> model != Uri.EMPTY
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