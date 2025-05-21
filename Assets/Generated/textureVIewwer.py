from PIL import Image
import numpy as np
import pandas as pd

img = Image.open("TerrainPatchesInvertedUVData.png").convert("RGBA")

width, height = img.size

# Convert image to numpy array (H, W, 4)
img_array = np.array(img)/ 255.0 

# Flip vertically so row 0 is bottom (graphics convention)
img_array = np.flipud(img_array)

# Extract R, G, A channels
R = img_array[:, :, 0]
G = img_array[:, :, 1]
A = img_array[:, :, 3]

# Create (R, G, A) tuple per pixel
RGA = np.stack((R, G, A), axis=-1)
RGA_tuples = np.apply_along_axis(lambda p: tuple(p), 2, RGA)

# Convert to DataFrame of tuples (each cell = (R, G, A))
df_rga = pd.DataFrame(RGA_tuples.tolist())

df_rga_inverted = df_rga.iloc[::-1].reset_index(drop=True)

# Save to CSV or inspect
df_rga_inverted.to_csv("rgba_pixel_table_inverted.csv", index=False)
print(df_rga_inverted.head())