set -e

# Below script will, manual clean, because the regular clean doesn't remove
# the intermediate project files created by 'nuget restore'

  for folder in */*/
  do
    [ -d "$folder"/obj ] && rm -rf "$folder"/obj && echo "Removed $folder/obj"
    [ -d "$folder"/bin ] && rm -rf "$folder"/bin && echo "Removed $folder/bin"
  done

# For Visual Studio project cache
  for folder in */
  do
    echo $folder
    rm -rf "$folder"/.vs
  done
