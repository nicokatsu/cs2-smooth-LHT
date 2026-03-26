import { ModRegistrar } from "cs2/modding";
import { InvertLHTTool } from "./mods/InvertLHTTool";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    moduleRegistry.extend(
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "MouseToolOptions",
        InvertLHTTool
    );
};

export default register;
